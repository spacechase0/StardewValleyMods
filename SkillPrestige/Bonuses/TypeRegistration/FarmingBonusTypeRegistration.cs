using System.Collections.Generic;
using SkillPrestige.Logging;

namespace SkillPrestige.Bonuses.TypeRegistration
{
    // ReSharper disable once UnusedMember.Global - created through reflection.
    public sealed class FarmingBonusTypeRegistration : BonusTypeRegistration
    {
        public override void RegisterBonusTypes()
        {
            Logger.LogInformation("Registering Farming bonus types...");
            FarmingToolProficiency = new BonusType
            {
                Code = "Farming1",
                Name = "Tool Proficiency",
                MaxLevel = 5,
                EffectDescriptions = new List<string>
                {
                    "+4 Hoe Proficiency.",
                    "+4 Watering Can Proficiency."
                },
                SkillType = SkillTypes.SkillType.Farming,
                ApplyEffect = x =>
                {
                    if (ToolProficiencyHandler.AddedToolProficencies.ContainsKey(ToolType.Hoe))
                    {
                        ToolProficiencyHandler.AddedToolProficencies[ToolType.Hoe] = x*4;
                    }
                    else
                    {
                        ToolProficiencyHandler.AddedToolProficencies.Add(ToolType.Hoe, x*4);
                    }
                    if (ToolProficiencyHandler.AddedToolProficencies.ContainsKey(ToolType.WateringCan))
                    {
                        ToolProficiencyHandler.AddedToolProficencies[ToolType.WateringCan] = x * 4;
                    }
                    else
                    {
                        ToolProficiencyHandler.AddedToolProficencies.Add(ToolType.WateringCan, x * 4);
                    }
                }
            };
            BetterCrops = new BonusType
            {
                Code = "Farming2",
                Name = "Better Crops",
                MaxLevel = 10,
                EffectDescriptions = new List<string>
                {
                    "+10% chance of better quality crop."
                },
                SkillType = SkillTypes.SkillType.Farming,
                ApplyEffect = x => CropQualityFactor.QualityImprovementChance = x/10m
            };
            EfficientAnimals = new BonusType
            {
                Code = "Farming3",
                Name = "Efficient Animals",
                MaxLevel = 5,
                EffectDescriptions = new List<string>
                {
                    "+20% chance of receiving double animal products."
                },
                SkillType = SkillTypes.SkillType.Farming,
                ApplyEffect = x =>AnimalProduceHandler.QuantityIncreaseChance = x/5m
            };
            RegrowthOpportunity = new BonusType
            {
                Code = "Farming4",
                Name = "Regrowth Opportunity",
                MaxLevel = 5,
                EffectDescriptions = new List<string>
                {
                    "+5% chance of receiving seeds with crops.",
                    "At max level, gives a 1/3 chance of receiving seeds from dead plants."
                },
                SkillType = SkillTypes.SkillType.Farming,
                ApplyEffect = x =>
                {
                    CropRegrowthFactor.RegrowthChance = x / 20m;
                    if (x == MaxLevel)
                    {
                        CropRegrowthFactor.DeadRegrowthChance = 1 / 3m;
                    }
                }
            };
            Logger.LogInformation("Farming bonus types registered.");
        }
    }
}
