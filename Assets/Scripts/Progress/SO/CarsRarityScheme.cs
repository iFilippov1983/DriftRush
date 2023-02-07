using Sirenix.OdinInspector;
using System;
using UnityEngine;
using RaceManager.Cars;
using System.Collections.Generic;

namespace RaceManager.Progress
{
    [Serializable]
    [CreateAssetMenu(menuName = "Progress/CarsRarityScheme", fileName = "CarsRarityScheme", order = 1)]
    public class CarsRarityScheme : SerializedScriptableObject
    {
        [DictionaryDrawerSettings(KeyLabel = "Rarity", ValueLabel = "Names")]
        public Dictionary<Rarity, List<CarName>> Scheme = new Dictionary<Rarity, List<CarName>>()
        {
            { 
                Rarity.Common, new List<CarName>() 
                { 
                    CarName.ToyotaSupra,
                    CarName.FordMustang
                } 
            },

            { 
                Rarity.Uncommon, new List<CarName>()
                { 
                    CarName.DodgeTRX,
                    CarName.NissanSilvia
                }
            },

            { 
                Rarity.Rare, new List<CarName>()
                { 
                    CarName.Porche911,
                    CarName.Ferrari488
                }
            },

            { 
                Rarity.Epic, new List<CarName>()
                { 
                    CarName.TeslaRoadster
                }
            },

            { 
                Rarity.Legendary, new List<CarName>()
                { 
                
                }
            }
        };
    }
}