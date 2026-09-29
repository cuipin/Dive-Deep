using System.Data;
using Dive_Deep.Data;
using Dive_Deep.Models;
using Dive_Deep.Services.Contracts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Dive_Deep.Persistence;

public class BookingRepository : IBookingRepository
{
    private readonly Dive_DeepContext _DiveDeepContext;

    public BookingRepository(Dive_DeepContext Database) => _DiveDeepContext = Database;

    public async Task<IReadOnlyList<Booking>> GetAllBookingsAsync(
        CancellationToken cancellationToken = default)
    // returns all bookings from every user so the admin can see them and manage them
    {
        return await _DiveDeepContext.Bookings
            .AsNoTracking()
            .Include(booking => booking.User)
            .Include(booking => booking.Items)
                .ThenInclude(item => item.ProductVariant)
                    .ThenInclude(variant => variant.Product)
                        .ThenInclude(product => product.Category)
            .OrderByDescending(booking => booking.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Booking>> GetForUserAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await _DiveDeepContext.Bookings
            .AsNoTracking()
            .Where(booking => booking.UserId == userId)
            .Include(booking => booking.User)
            .Include(booking => booking.Items)
                .ThenInclude(item => item.ProductVariant)
                    .ThenInclude(variant => variant.Product)
                        .ThenInclude(product => product.Category)
            .OrderByDescending(booking => booking.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public Task<Booking?> GetForUserAsync(
        int bookingId,
        string? userId,
        CancellationToken cancellationToken = default)
    {
        return _DiveDeepContext.Bookings
            .AsNoTracking()
            .Where(booking => booking.BookingId == bookingId
                && (userId == null || booking.UserId == userId))
            .Include(booking => booking.User)
            .Include(booking => booking.Items)
                .ThenInclude(item => item.ProductVariant)
                    .ThenInclude(variant => variant.Product)
                        .ThenInclude(product => product.Category)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<BookingReservationOutcome> CreateWithAllocationsAsync(
        Booking booking,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _DiveDeepContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var selectedUnits = new List<(int UnitId, DateTimeOffset Start, DateTimeOffset End)>();

        foreach (var item in booking.Items)
        {
            var variant = await _DiveDeepContext.ProductVariants
                .AsNoTracking()
                .Where(candidate => candidate.ProductVariantId == item.ProductVariantId
                    && candidate.IsActive
                    && candidate.Product.IsActive)
                .Select(candidate => new { candidate.DailyRate })
                .SingleOrDefaultAsync(cancellationToken);

            if (variant is null)
            {
                return BookingReservationOutcome.ProductVariantNotFound;
            }

            item.DailyRateAtBooking = variant.DailyRate;
            var conflictingSelectedIds = selectedUnits
                .Where(selected => selected.Start < item.EndTime && selected.End > item.StartTime)
                .Select(selected => selected.UnitId)
                .ToArray();

            List<int> unitIds;
            try
            {
                unitIds = await _DiveDeepContext.EquipmentUnits
                    .AsNoTracking()
                    .Where(unit => unit.ProductVariantId == item.ProductVariantId
                        && unit.Status == EquipmentUnitStatus.Active
                        && !conflictingSelectedIds.Contains(unit.EquipmentUnitId))
                    .Where(unit => !_DiveDeepContext.BookingAllocations.Any(allocation =>
                        allocation.EquipmentUnitId == unit.EquipmentUnitId
                        && allocation.BookingItem.Booking.Status != BookingStatus.Cancelled
                        && allocation.BookingItem.StartTime < item.EndTime
                        && allocation.BookingItem.EndTime > item.StartTime))
                    .OrderBy(unit => unit.AssetTag)
                    .Select(unit => unit.EquipmentUnitId)
                    .Take(item.Quantity)
                    .ToListAsync(cancellationToken);
            }
            catch (SqlException exception) when (exception.Number == 1205)
            {
                await transaction.RollbackAsync(cancellationToken);
                return BookingReservationOutcome.Unavailable;
            }

            if (unitIds.Count < item.Quantity)
            {
                return BookingReservationOutcome.Unavailable;
            }

            foreach (var unitId in unitIds)
            {
                item.Allocations.Add(new BookingAllocation
                {
                    BookingItem = item,
                    EquipmentUnitId = unitId
                });
                selectedUnits.Add((unitId, item.StartTime, item.EndTime));
            }
        }

        _DiveDeepContext.Bookings.Add(booking);
        try
        {
            await _DiveDeepContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return BookingReservationOutcome.Reserved;
        }
        catch (DbUpdateException exception) when (IsInventoryRace(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return BookingReservationOutcome.Unavailable;
        }
    }

    public async Task<BookingReservationOutcome> UpdateSingleLineAsync(
        int bookingId,
        int bookingItemId,
        string? userId,
        bool isAdmin,
        BookingLineRequest line,
        CancellationToken cancellationToken = default)
    {
        await using var transaction = await _DiveDeepContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var booking = await _DiveDeepContext.Bookings
            .Include(candidate => candidate.Items)
                .ThenInclude(item => item.Allocations)
            .SingleOrDefaultAsync(
                candidate => candidate.BookingId == bookingId
                    && (isAdmin || candidate.UserId == userId),
                cancellationToken);

        if (booking is null || booking.Status != BookingStatus.Confirmed)
        {
            return BookingReservationOutcome.BookingNotFound;
        }

        var item = booking.Items.SingleOrDefault(candidate => candidate.BookingItemId == bookingItemId);
        if (item is null)
        {
            return BookingReservationOutcome.BookingNotFound;
        }
        var variant = await _DiveDeepContext.ProductVariants
            .AsNoTracking()
            .Where(candidate => candidate.ProductVariantId == line.ProductVariantId
                && candidate.IsActive
                && candidate.Product.IsActive)
            .Select(candidate => new { candidate.DailyRate })
            .SingleOrDefaultAsync(cancellationToken);

        if (variant is null)
        {
            return BookingReservationOutcome.ProductVariantNotFound;
        }

        _DiveDeepContext.BookingAllocations.RemoveRange(item.Allocations);
        try
        {
            await _DiveDeepContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsInventoryRace(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return BookingReservationOutcome.Unavailable;
        }

        item.ProductVariantId = line.ProductVariantId;
        item.Quantity = line.Quantity;
        item.StartTime = line.StartTime;
        item.EndTime = line.EndTime;
        item.DailyRateAtBooking = variant.DailyRate;
        booking.UpdatedAtUtc = DateTimeOffset.UtcNow;

        List<int> unitIds;
        try
        {
            unitIds = await _DiveDeepContext.EquipmentUnits
                .AsNoTracking()
                .Where(unit => unit.ProductVariantId == line.ProductVariantId
                    && unit.Status == EquipmentUnitStatus.Active)
                .Where(unit => !_DiveDeepContext.BookingAllocations.Any(allocation =>
                    allocation.EquipmentUnitId == unit.EquipmentUnitId
                    && allocation.BookingItemId != bookingItemId
                    && allocation.BookingItem.Booking.Status != BookingStatus.Cancelled
                    && allocation.BookingItem.StartTime < line.EndTime
                    && allocation.BookingItem.EndTime > line.StartTime))
                .OrderBy(unit => unit.AssetTag)
                .Select(unit => unit.EquipmentUnitId)
                .Take(line.Quantity)
                .ToListAsync(cancellationToken);
        }
        catch (SqlException exception) when (exception.Number == 1205)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BookingReservationOutcome.Unavailable;
        }

        if (unitIds.Count < line.Quantity)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BookingReservationOutcome.Unavailable;
        }

        foreach (var unitId in unitIds)
        {
            item.Allocations.Add(new BookingAllocation
            {
                BookingItem = item,
                EquipmentUnitId = unitId
            });
        }

        try
        {
            await _DiveDeepContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return BookingReservationOutcome.Reserved;
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            return BookingReservationOutcome.ConcurrencyConflict;
        }
        catch (DbUpdateException exception) when (IsInventoryRace(exception))
        {
            await transaction.RollbackAsync(cancellationToken);
            return BookingReservationOutcome.Unavailable;
        }
    }

    public async Task<BookingReservationOutcome> CancelAsync(
        int bookingId,
        string userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var booking = await _DiveDeepContext.Bookings.SingleOrDefaultAsync(
            candidate => candidate.BookingId == bookingId
                && (isAdmin || candidate.UserId == userId),
            cancellationToken);

        if (booking is null)
        {
            return BookingReservationOutcome.BookingNotFound;
        }

        if (booking.Status == BookingStatus.Cancelled)
        {
            return BookingReservationOutcome.Reserved;
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAtUtc = DateTimeOffset.UtcNow;

        try
        {
            await _DiveDeepContext.SaveChangesAsync(cancellationToken);
            return BookingReservationOutcome.Reserved;
        }
        catch (DbUpdateConcurrencyException)
        {
            return BookingReservationOutcome.ConcurrencyConflict;
        }
    }

    private static bool IsInventoryRace(DbUpdateException exception) =>
        exception.GetBaseException() is SqlException { Number: 1205 or 2601 or 2627 };
}
