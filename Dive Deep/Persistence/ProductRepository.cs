using System.Collections.Generic;
using System.Linq;
using Dive_Deep.Models;
using Dive_Deep.Models.ProduktTypes;

namespace Dive_Deep.Persistence
{
    public static class ProductRepository
    {
        private static List<Product> products = new List<Product>
           {
                new BCD
                {
                    Category = "BCD",
                    Mærke = "Scubapro",
                    PrisPrDag = 125,
                    Model = "Navigator Lite BCD",
                    Størrelse = "S, M, L"

                },

                new BCD
                {
                    Category = "BCD",
                    Mærke = "Scubapro",
                    PrisPrDag = 140,
                    Model = "BCD Glide",
                    Størrelse = "S, M, L"

                },

                new BCD
                {
                    Category = "BCD",
                    Mærke = "Scubapro",
                    PrisPrDag = 200,
                    Model = "BCD Hydros Pro",
                    Størrelse = "S, M, L"

                },

                new BCD
                {
                    Category = "BCD",
                    Mærke = "Seac",
                    PrisPrDag = 145,
                    Model = "BCD Modular",
                    Størrelse = "S, M, L"

                },

                new Dykkerdragter
                {
                    Category = "Dykkerdragt",
                    Mærke = "Scubapro",
                    PrisPrDag = 100,
                    Model = "Definition",
                    Størrelse = "XS, S, M, L, XL",
                    Type = "Våddragt",
                    Køn = "Unisex",
                    Tykkelse = 3
                },

               new Dykkerdragter
                {
                    Category = "Dykkerdragt",
                    Mærke = "Scubapro",
                    PrisPrDag = 100,
                    Model = "Definition",
                    Størrelse = "XS, S, M, L, XL",
                    Type = "Våddragt",
                    Køn = "Unisex",
                    Tykkelse = 5
                },

               new Dykkerdragter
                {
                    Category = "Dykkerdragt",
                    Mærke = "Scubapro",
                    PrisPrDag = 100,
                    Model = "Definition",
                    Størrelse = "XS, S, M, L, XL",
                    Type = "Våddragt",
                    Køn = "Unisex",
                    Tykkelse = 7
                },

               new Dykkerdragter
                {
                    Category = "Dykkerdragt",
                    Mærke = "Waterproof",
                    PrisPrDag = 100,
                    Model = "W5",
                    Størrelse = "XS, S, M, L, XL",
                    Type = "Våddragt",
                    Køn = "Unisex",
                    Tykkelse = 3.5
                },

               new Dykkerdragter
                {
                    Category = "Dykkerdragt",
                    Mærke = "Fourth Element",
                    PrisPrDag = 120,
                    Model = "Proteus",
                    Størrelse = "XS, S, M, L, XL",
                    Type = "Våddragt",
                    Køn = "Unisex",
                    Tykkelse = 5
                },

               new Dykkerdragter
                {
                    Category = "Dykkerdragt",
                    Mærke = "Scubapro",
                    PrisPrDag = 300,
                    Model = "Exodry 4.0",
                    Størrelse = "XS, S, M, L, XL",
                    Type = "Tørdragt",
                    Køn = "Unisex",
                    Tykkelse = null
                },

                new Dykkerdragter
                {
                    Category = "Dykkerdragt",
                    Mærke = "Waterproof",
                    PrisPrDag = 320,
                    Model = "D7 Evo",
                    Størrelse = "XS, S, M, L, XL",
                    Type = "Tørdragt",
                    Køn = "Unisex",
                    Tykkelse = null
                },

                 new Dykkerdragter
                {
                    Category = "Dykkerdragt",
                    Mærke = "Santi",
                    PrisPrDag = 350,
                    Model = "E.Lite Plus",
                    Størrelse = "XS, S, M, L, XL",
                    Type = "Tørdragt",
                    Køn = "Unisex",
                    Tykkelse = null
                },

                 new Tanke
                {
                    Category = "Tanke",
                    Mærke = "Scubapro",
                    PrisPrDag = 150,
                    Volumen = 5
                },

                  new Tanke
                {
                    Category = "Tanke",
                    Mærke = "Scubapro",
                    PrisPrDag = 160,
                    Volumen = 10
                },

                   new Tanke
                {
                    Category = "Tanke",
                    Mærke = "Scubapro",
                    PrisPrDag = 170,
                    Volumen = 12
                },

                   new Tanke
                {
                    Category = "Tanke",
                    Mærke = "Scubapro",
                    PrisPrDag = 180,
                    Volumen = 15
                },

                   new Regulatorsæt
                {
                    Category = "Regulatorsæt",
                    Mærke = "Scubapro",
                    PrisPrDag = 125,
                    Trin1 = "MK25EVO",
                    Trin2 = "S600",
                    Octopus = "R105"
                },

                  new Regulatorsæt
                {
                    Category = "Regulatorsæt",
                    Mærke = "Scubapro",
                    PrisPrDag = 100,
                    Trin1 = "MK17EVO",
                    Trin2 = "C370",
                    Octopus = "R095"
                },

                  new Regulatorsæt
                {
                    Category = "Regulatorsæt",
                    Mærke = "Scubapro",
                    PrisPrDag = 150,
                    Trin1 = "MK25EVO BT",
                    Trin2 = "A700 Carbon BT",
                    Octopus = "S270"
                },

                  new MaskeSnorkel
                  {
                    Category = "Maske/Snorkel",
                    Mærke = "Scubapro",
                    PrisPrDag = 50,
                    Model = "Ghost",
                  },

                  new MaskeSnorkel
                  {
                    Category = "Maske/Snorkel",
                    Mærke = "Scubapro",
                    PrisPrDag = 60,
                    Model = "D-Mask",
                  },

                  new MaskeSnorkel
                  {
                    Category = "Maske/Snorkel",
                    Mærke = "Scubapro",
                    PrisPrDag = 50,
                    Model = "Spectra Mini",
                  },

                  new MaskeSnorkel
                  {
                    Category = "Maske/Snorkel",
                    Mærke = "Scubapro",
                    PrisPrDag = 75,
                    Model = "Crystal VU",
                  },

                  new MaskeSnorkel
                  {
                    Category = "Maske/Snorkel",
                    Mærke = "Fourth Element",
                    PrisPrDag = 75,
                    Model = "Scout Kontrast",
                  },

                  new MaskeSnorkel
                  {
                    Category = "Maske/Snorkel",
                    Mærke = "Fourth Element",
                    PrisPrDag = 75,
                    Model = "Scout Enhance",
                  },

                  new MaskeSnorkel
                  {
                    Category = "Maske/Snorkel",
                    Mærke = "Tusa",
                    PrisPrDag = 75,
                    Model = "Element",
                  },

                  new Finner
                  {
                    Category = "Finner",
                    Mærke = "Scubapro",
                    PrisPrDag = 50,
                    Model = "Jet Fin",
                    Størrelse = "XS, S, M, L, XL"
                  },

                  new Finner
                  {
                    Category = "Finner",
                    Mærke = "Scubapro",
                    PrisPrDag = 50,
                    Model = "GO Travel",
                    Størrelse = "XS, S, M, L, XL"
                  },

                  new Finner
                  {
                    Category = "Finner",
                    Mærke = "Scubapro",
                    PrisPrDag = 60,
                    Model = "Seawing Supernova",
                    Størrelse = "XS, S, M, L, XL"
                  },

                  new Finner
                  {
                    Category = "Finner",
                    Mærke = "Seac",
                    PrisPrDag = 50,
                    Model = "Propulsion",
                    Størrelse = "XS, S, M, L, XL"
                  },

                  new Finner
                  {
                    Category = "Finner",
                    Mærke = "Seac",
                    PrisPrDag = 50,
                    Model = "ALA",
                    Størrelse = "XS, S, M, L, XL"
                  },

                  new Finner
                  {
                    Category = "Finner",
                    Mærke = "Fourth Element",
                    PrisPrDag = 75,
                    Model = "Tech",
                    Størrelse = "XS, S, M, L, XL"
                  },

                  new Finner
                  {
                    Category = "Finner",
                    Mærke = "Fourth Element",
                    PrisPrDag = 80,
                    Model = "Rec Fin",
                    Størrelse = "XS, S, M, L, XL"
                  },
           };

        public static List<Product> GetAll()
        {
            return products;
        }

        // New helper to get all items of a specific derived type
        public static IEnumerable<T> GetAllOfType<T>() where T : Product
        {
            return products.OfType<T>();
        }
    }
}
