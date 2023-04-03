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
                    CarName.SuperBull,
                    CarName.BlinkGoat
                } 
            },

            { 
                Rarity.Uncommon, new List<CarName>()
                { 
                    CarName.HyperWolf,
                    CarName.ThaurenG86
                }
            },

            { 
                Rarity.Rare, new List<CarName>()
                { 
                    CarName.ChivalryS1,
                    CarName.Mosquito_3
                }
            },

            { 
                Rarity.Epic, new List<CarName>()
                { 
                    CarName.BearRod
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