/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using Chummer.Backend.Attributes;
using Chummer.Backend.Skills;
using Chummer.Backend.Uniques;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Chummer
{
    [DebuggerDisplay("{" + nameof(DisplayDebug) + "()}")]
    public class Improvement : IHasNotes, IHasInternalId, ICanSort
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;

        private string DisplayDebug()
        {
            return string.Format(GlobalSettings.InvariantCultureInfo, "{0} ({1}, {2}) 🡐 {3}, {4}, {5}",
                                 _eImprovementType, _decVal, _intRating, _eImprovementSource, _strSourceName,
                                 _strImprovedName);
        }

        public enum ImprovementType
        {
            None,
            Attribute,
            Text,
            Armor,
            FireArmor,
            ColdArmor,
            ElectricityArmor,
            AcidArmor,
            FallingArmor,
            Dodge,
            Reach,
            Nuyen,
            NuyenExpense,
            PhysicalCM,
            StunCM,
            UnarmedDV,
            InitiativeDice,
            MatrixInitiative,
            MatrixInitiativeDice,
            LifestyleCost,
            CMThreshold,
            EnhancedArticulation,
            WeaponCategoryDV,
            WeaponCategoryDice,
            WeaponSpecificDice,
            CyberwareEssCost,
            CyberwareTotalEssMultiplier,
            CyberwareEssCostNonRetroactive,
            CyberwareTotalEssMultiplierNonRetroactive,
            SpecialTab,
            Initiative,
            LivingPersonaDeviceRating,
            LivingPersonaProgramLimit,
            LivingPersonaAttack,
            LivingPersonaSleaze,
            LivingPersonaDataProcessing,
            LivingPersonaFirewall,
            LivingPersonaMatrixCM,
            Smartlink,
            BiowareEssCost,
            BiowareTotalEssMultiplier,
            BiowareEssCostNonRetroactive,
            BiowareTotalEssMultiplierNonRetroactive,
            GenetechCostMultiplier,
            BasicBiowareEssCost,
            SoftWeave,
            DisableBioware,
            DisableCyberware,
            DisableBiowareGrade,
            DisableCyberwareGrade,
            ConditionMonitor,
            UnarmedDVPhysical,
            Adapsin,
            FreePositiveQualities,
            FreeNegativeQualities,
            FreeKnowledgeSkills,
            NuyenMaxBP,
            CMOverflow,
            FreeSpiritPowerPoints,
            AdeptPowerPoints,
            ArmorEncumbrancePenalty,
            Art,
            Metamagic,
            Echo,
            Skillwire,
            DamageResistance,
            JudgeIntentions,
            JudgeIntentionsOffense,
            JudgeIntentionsDefense,
            LiftAndCarry,
            Memory,
            Concealability,
            SwapSkillAttribute,
            DrainResistance,
            FadingResistance,
            MatrixInitiativeDiceAdd,
            InitiativeDiceAdd,
            Composure,
            UnarmedAP,
            CMThresholdOffset,
            CMSharedThresholdOffset,
            Restricted,
            Notoriety,
            SpellCategory,
            SpellCategoryDamage,
            SpellCategoryDrain,
            SpellDescriptorDamage,
            SpellDescriptorDrain,
            SpellDicePool,
            ThrowRange,
            ThrowRangeSTR,
            SkillsoftAccess,
            AddSprite,
            BlackMarketDiscount,
            ComplexFormLimit,
            SpellLimit,
            QuickeningMetamagic,
            BasicLifestyleCost,
            ThrowSTR,
            IgnoreCMPenaltyStun,
            IgnoreCMPenaltyPhysical,
            CyborgEssence,
            EssenceMax,
            AdeptPower,
            SpecificQuality,
            MartialArt,
            LimitModifier,
            PhysicalLimit,
            MentalLimit,
            SocialLimit,
            FriendsInHighPlaces,
            Erased,
            Fame,
            MadeMan,
            Overclocker,
            RestrictedGear,
            TrustFund,
            ExCon,
            ContactForceGroup,
            Attributelevel,
            AddContact,
            Seeker,
            PublicAwareness,
            PrototypeTranshuman,
            Hardwire,
            DealerConnection,
            Skill, //Improve pool of skill based on name
            SkillGroup, //Group
            SkillCategory, //category
            SkillAttribute, //attribute
            SkillLinkedAttribute, //linked attribute
            SkillLevel, //Karma points in skill
            SkillGroupLevel, //group
            SkillBase, //base points in skill
            SkillGroupBase, //group
            Skillsoft, // A knowledge or language skill gained from a knowsoft
            Activesoft, // An active skill gained from an activesoft
            ReplaceAttribute, //Alter the base metatype or metavariant of a character. Used for infected.
            SpecialSkills,
            ReflexRecorderOptimization,
            BlockSkillCategoryDefault,
            BlockSkillGroupDefault,
            BlockSkillDefault,
            AllowSkillDefault,
            Ambidextrous,
            UnarmedReach,
            SkillSpecialization,
            SkillExpertise, // SASS' Inspired, adds a specialization that gives a +3 bonus instead of the usual +2
            SkillSpecializationOption,
            NativeLanguageLimit,
            AdeptPowerFreeLevels,
            AdeptPowerFreePoints,
            AIProgram,
            CritterPowerLevel,
            CritterPower,
            SwapSkillSpecAttribute,
            SpellResistance,
            AllowSpellCategory,
            LimitSpellCategory,
            AllowSpellRange,
            LimitSpellRange,
            BlockSpellDescriptor,
            LimitSpellDescriptor,
            LimitSpiritCategory,
            WalkSpeed,
            RunSpeed,
            SprintSpeed,
            WalkMultiplier,
            RunMultiplier,
            SprintBonus,
            WalkMultiplierPercent,
            RunMultiplierPercent,
            SprintBonusPercent,
            EssencePenalty,
            EssencePenaltyT100,
            EssencePenaltyMAGOnlyT100,
            EssencePenaltyRESOnlyT100,
            EssencePenaltyDEPOnlyT100,
            SpecialAttBurn,
            SpecialAttTotalBurnMultiplier,
            FreeSpellsATT,
            FreeSpells,
            DrainValue,
            FadingValue,
            Spell,
            ComplexForm,
            Gear,
            Weapon,
            MentorSpirit,
            Paragon,
            FreeSpellsSkill,
            DisableSpecializationEffects, // Disable the effects of specializations for a skill
            FatigueResist,
            RadiationResist,
            SonicResist,
            ToxinContactResist,
            ToxinIngestionResist,
            ToxinInhalationResist,
            ToxinInjectionResist,
            PathogenContactResist,
            PathogenIngestionResist,
            PathogenInhalationResist,
            PathogenInjectionResist,
            ToxinContactImmune,
            ToxinIngestionImmune,
            ToxinInhalationImmune,
            ToxinInjectionImmune,
            PathogenContactImmune,
            PathogenIngestionImmune,
            PathogenInhalationImmune,
            PathogenInjectionImmune,
            PhysiologicalAddictionFirstTime,
            PsychologicalAddictionFirstTime,
            PhysiologicalAddictionAlreadyAddicted,
            PsychologicalAddictionAlreadyAddicted,
            StunCMRecovery,
            PhysicalCMRecovery,
            AddESStoStunCMRecovery,
            AddESStoPhysicalCMRecovery,
            MentalManipulationResist,
            PhysicalManipulationResist,
            ManaIllusionResist,
            PhysicalIllusionResist,
            DetectionSpellResist,
            DirectManaSpellResist,
            DirectPhysicalSpellResist,
            DecreaseBODResist,
            DecreaseAGIResist,
            DecreaseREAResist,
            DecreaseSTRResist,
            DecreaseCHAResist,
            DecreaseINTResist,
            DecreaseLOGResist,
            DecreaseWILResist,
            AddLimb,
            StreetCredMultiplier,
            StreetCred,
            AttributeKarmaCostMultiplier,
            AttributeKarmaCost,
            ActiveSkillKarmaCostMultiplier,
            SkillGroupKarmaCostMultiplier,
            KnowledgeSkillKarmaCostMultiplier,
            ActiveSkillKarmaCost,
            SkillGroupKarmaCost,
            SkillGroupDisable,
            SkillDisable,
            KnowledgeSkillKarmaCost,
            KnowledgeSkillKarmaCostMinimum,
            SkillCategorySpecializationKarmaCostMultiplier,
            SkillCategorySpecializationKarmaCost,
            SkillCategoryKarmaCostMultiplier,
            SkillCategoryKarmaCost,
            SkillGroupCategoryKarmaCostMultiplier,
            SkillGroupCategoryDisable,
            SkillGroupCategoryKarmaCost,
            AttributePointCostMultiplier,
            AttributePointCost,
            ActiveSkillPointCostMultiplier,
            SkillGroupPointCostMultiplier,
            KnowledgeSkillPointCostMultiplier,
            ActiveSkillPointCost,
            SkillGroupPointCost,
            KnowledgeSkillPointCost,
            SkillCategoryPointCostMultiplier,
            SkillCategoryPointCost,
            SkillGroupCategoryPointCostMultiplier,
            SkillGroupCategoryPointCost,
            NewSpellKarmaCostMultiplier,
            NewSpellKarmaCost,
            NewComplexFormKarmaCostMultiplier,
            NewComplexFormKarmaCost,
            NewAIProgramKarmaCostMultiplier,
            NewAIProgramKarmaCost,
            NewAIAdvancedProgramKarmaCostMultiplier,
            NewAIAdvancedProgramKarmaCost,
            BlockSkillSpecializations,
            BlockSkillCategorySpecializations,
            FocusBindingKarmaCost,
            FocusBindingKarmaMultiplier,
            MagiciansWayDiscount,
            BurnoutsWay,
            ContactForcedLoyalty,
            ContactMakeFree,
            FreeWare,
            WeaponAccuracy,
            WeaponSkillAccuracy,
            MetageneticLimit,
            Tradition,
            ActionDicePool,
            SpecialModificationLimit,
            AddSpirit,
            ContactKarmaDiscount,
            ContactKarmaMinimum,
            GenetechEssMultiplier,
            AllowSpriteFettering,
            DisableDrugGrade,
            DrugDuration,
            DrugDurationMultiplier,
            Surprise,
            EnableCyberzombie,
            AllowCritterPowerCategory,
            LimitCritterPowerCategory,
            AttributeMaxClamp,
            MetamagicLimit,
            DisableQuality,
            FreeQuality,
            AstralReputation,
            AstralReputationWild,
            CyberadeptDaemon,
            PenaltyFreeSustain,
            WeaponRangeModifier,
            ReplaceSkillSpell,
            NumImprovementTypes // 🡐 This one should always be the last defined enum
        }

        public enum ImprovementSource
        {
            Quality,
            Power,
            Metatype,
            Cyberware,
            Metavariant,
            Bioware,
            ArmorEncumbrance,
            Gear,
            VehicleMod,
            Spell,
            Initiation,
            Submersion,
            Metamagic,
            Echo,
            Armor,
            ArmorMod,
            EssenceLoss,
            EssenceLossChargen,
            CritterPower,
            ComplexForm,
            MutantCritter,
            Cyberzombie,
            StackedFocus,
            AttributeLoss,
            Art,
            Enhancement,
            Custom,
            Heritage,
            MartialArt,
            MartialArtTechnique,
            AIProgram,
            SpiritFettering,
            MentorSpirit,
            Drug,
            Tradition,
            Weapon,
            WeaponAccessory,
            AstralReputation,
            CyberadeptDaemon,
            BurnedEdge,
            Encumbrance,
            NumImprovementSources // 🡐 This one should always be the last defined enum
        }

        private readonly Character _objCharacter;
        private string _strImprovedName = string.Empty;
        private string _strSourceName = string.Empty;
        private int _intMin;
        private int _intMax;
        private decimal _decAug;
        private int _intAugMax;
        private decimal _decVal;
        private int _intRating = 1;
        private string _strExclude = string.Empty;
        private string _strCondition = string.Empty;
        private string _strUniqueName = string.Empty;
        private string _strTarget = string.Empty;
        private ImprovementType _eImprovementType;
        private ImprovementSource _eImprovementSource;
        private bool _blnCustom;
        private string _strCustomName = string.Empty;
        private string _strCustomId = string.Empty;
        private string _strCustomGroup = string.Empty;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private int _intAddToRating;
        private int _intEnabled = 1;

        // Start with Improvement disabled, then enable it after all properties are set up at creation
        private bool _blnSetupComplete;

        private int _intOrder;

        #region Helper Methods

        /// <summary>
        /// Convert a string to an ImprovementType.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static ImprovementType ConvertToImprovementType(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
                return ImprovementType.None;
            if (strValue.Contains("InitiativePass"))
            {
                strValue = strValue.Replace("InitiativePass", "InitiativeDice");
            }

            if (strValue == "ContactForceLoyalty")
                strValue = "ContactForcedLoyalty";
            return (ImprovementType)Enum.Parse(typeof(ImprovementType), strValue);
        }

        /// <summary>
        /// Convert a string to an ImprovementSource.
        /// </summary>
        /// <param name="strValue">String value to convert.</param>
        public static ImprovementSource ConvertToImprovementSource(string strValue)
        {
            if (strValue == "MartialArtAdvantage")
                strValue = "MartialArtTechnique";
            return (ImprovementSource)Enum.Parse(typeof(ImprovementSource), strValue);
        }

        #endregion Helper Methods

        #region Save and Load Methods

        public Improvement(Character objCharacter)
        {
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            Log.Trace("Save enter");

            objWriter.WriteStartElement("improvement");
            if (!string.IsNullOrEmpty(_strUniqueName))
                objWriter.WriteElementString("unique", _strUniqueName);
            objWriter.WriteElementString("target", _strTarget);
            objWriter.WriteElementString("improvedname", _strImprovedName);
            objWriter.WriteElementString("sourcename", _strSourceName);
            objWriter.WriteElementString("min", _intMin.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("max", _intMax.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("aug", _decAug.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("augmax", _intAugMax.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("val", _decVal.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("rating", _intRating.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("exclude", _strExclude);
            objWriter.WriteElementString("condition", _strCondition);
            objWriter.WriteElementString("improvementttype", _eImprovementType.ToString());
            objWriter.WriteElementString("improvementsource", _eImprovementSource.ToString());
            objWriter.WriteElementString("custom", _blnCustom.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("customname", _strCustomName);
            objWriter.WriteElementString("customid", _strCustomId);
            objWriter.WriteElementString("customgroup", _strCustomGroup);
            objWriter.WriteElementString("addtorating", _intAddToRating.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("enabled", _intEnabled.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("order", _intOrder.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteEndElement();

            Log.Trace("Save end");
        }

        /// <summary>
        /// Load the CharacterAttribute from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            Log.Trace("Load enter");

            objNode.TryGetStringFieldQuickly("unique", ref _strUniqueName);
            objNode.TryGetStringFieldQuickly("target", ref _strTarget);
            objNode.TryGetStringFieldQuickly("improvedname", ref _strImprovedName);
            objNode.TryGetStringFieldQuickly("sourcename", ref _strSourceName);
            objNode.TryGetInt32FieldQuickly("min", ref _intMin);
            objNode.TryGetInt32FieldQuickly("max", ref _intMax);
            objNode.TryGetDecFieldQuickly("aug", ref _decAug);
            objNode.TryGetInt32FieldQuickly("augmax", ref _intAugMax);
            objNode.TryGetDecFieldQuickly("val", ref _decVal);
            objNode.TryGetInt32FieldQuickly("rating", ref _intRating);
            objNode.TryGetStringFieldQuickly("exclude", ref _strExclude);
            objNode.TryGetStringFieldQuickly("condition", ref _strCondition);
            if (objNode["improvementttype"] != null)
                _eImprovementType = ConvertToImprovementType(objNode["improvementttype"].InnerText);
            if (objNode["improvementsource"] != null)
                _eImprovementSource = ConvertToImprovementSource(objNode["improvementsource"].InnerText);
            // Legacy shims
            if (_objCharacter.LastSavedVersion <= new Version(5, 214, 112)
                && (_eImprovementSource == ImprovementSource.Initiation
                    || _eImprovementSource == ImprovementSource.Submersion)
                && _eImprovementType == ImprovementType.Attribute
                && _intMax > 1 && _intRating == 1)
            {
                _intRating = _intMax;
                _intMax = 1;
            }

            switch (_eImprovementType)
            {
                case ImprovementType.LimitModifier
                    when string.IsNullOrEmpty(_strCondition) && !string.IsNullOrEmpty(_strExclude):
                    _strCondition = _strExclude;
                    _strExclude = string.Empty;
                    break;

                case ImprovementType.RestrictedGear when _decVal == 0:
                    _decVal = 24;
                    break;

                case ImprovementType.BlockSkillDefault when _objCharacter.LastSavedVersion <= new Version(5, 224, 39):
                    _eImprovementType = ImprovementType.BlockSkillGroupDefault;
                    break;
            }

            objNode.TryGetBoolFieldQuickly("custom", ref _blnCustom);
            objNode.TryGetStringFieldQuickly("customname", ref _strCustomName);
            objNode.TryGetStringFieldQuickly("customid", ref _strCustomId);
            objNode.TryGetStringFieldQuickly("customgroup", ref _strCustomGroup);
            if (objNode.TryGetInt32FieldQuickly("addtorating", ref _intAddToRating))
            {
                bool blnTemp = false;
                if (objNode.TryGetBoolFieldQuickly("addtorating", ref blnTemp))
                    _intAddToRating = blnTemp.ToInt32();
            }
            if (objNode.TryGetInt32FieldQuickly("enabled", ref _intEnabled))
            {
                bool blnTemp = false;
                if (objNode.TryGetBoolFieldQuickly("enabled", ref blnTemp))
                    _intEnabled = blnTemp.ToInt32();
            }
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            objNode.TryGetInt32FieldQuickly("order", ref _intOrder);

            Log.Trace("Load exit");
        }

        #endregion Save and Load Methods

        #region Properties

        /// <summary>
        /// Whether or not this is a custom-made (manually created) Improvement.
        /// </summary>
        public bool Custom
        {
            get => _blnCustom;
            set => _blnCustom = value;
        }

        /// <summary>
        /// User-entered name for the custom Improvement.
        /// </summary>
        public string CustomName
        {
            get => _strCustomName;
            set => _strCustomName = value;
        }

        /// <summary>
        /// ID from the Improvements file. Only used for custom-made (manually created) Improvements.
        /// </summary>
        public string CustomId
        {
            get => _strCustomId;
            set => _strCustomId = value;
        }

        /// <summary>
        /// Group name for the Custom Improvement.
        /// </summary>
        public string CustomGroup
        {
            get => _strCustomGroup;
            set => _strCustomGroup = value;
        }

        /// <summary>
        /// User-entered notes for the custom Improvement.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get => _colNotes;
            set => _colNotes = value;
        }

        /// <summary>
        /// Name of the Skill or CharacterAttribute that the Improvement is improving.
        /// </summary>
        public string ImprovedName
        {
            get => _strImprovedName;
            set
            {
                string strOldValue = Interlocked.Exchange(ref _strImprovedName, value);
                if (strOldValue != value && Enabled)
                {
                    ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, strOldValue);
                    ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, value);
                    this.ProcessRelevantEvents(lstExtraImprovedName: strOldValue.Yield().ToList());
                }
            }
        }

        /// <summary>
        /// Name of the source that granted this Improvement.
        /// </summary>
        public string SourceName
        {
            get => _strSourceName;
            set => _strSourceName = value;
        }

        /// <summary>
        /// The type of Object that the Improvement is improving.
        /// </summary>
        public ImprovementType ImproveType
        {
            get => _eImprovementType;
            set
            {
                ImprovementType eOldType = InterlockedExtensions.Exchange(ref _eImprovementType, value);
                if (eOldType != value && Enabled)
                {
                    ImprovementManager.ClearCachedValue(_objCharacter, eOldType, ImprovedName);
                    ImprovementManager.ClearCachedValue(_objCharacter, value, ImprovedName);
                    this.ProcessRelevantEvents(lstExtraImprovementTypes: eOldType.Yield());
                }
            }
        }

        /// <summary>
        /// The type of Object that granted this Improvement.
        /// </summary>
        public ImprovementSource ImproveSource
        {
            get => _eImprovementSource;
            set
            {
                if (InterlockedExtensions.Exchange(ref _eImprovementSource, value) != value && Enabled)
                {
                    ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                    this.ProcessRelevantEvents();
                }
            }
        }

        /// <summary>
        /// Minimum value modifier.
        /// </summary>
        public int Minimum
        {
            get => _intMin;
            set
            {
                if (Interlocked.Exchange(ref _intMin, value) != value && Enabled)
                {
                    ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                    this.ProcessRelevantEvents();
                }
            }
        }

        /// <summary>
        /// Maximum value modifier.
        /// </summary>
        public int Maximum
        {
            get => _intMax;
            set
            {
                if (Interlocked.Exchange(ref _intMax, value) != value && Enabled)
                {
                    ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                    this.ProcessRelevantEvents();
                }
            }
        }

        /// <summary>
        /// Augmented Maximum value modifier.
        /// </summary>
        public int AugmentedMaximum
        {
            get => _intAugMax;
            set
            {
                if (Interlocked.Exchange(ref _intAugMax, value) != value && Enabled)
                {
                    ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                    this.ProcessRelevantEvents();
                }
            }
        }

        /// <summary>
        /// Augmented score modifier.
        /// </summary>
        public decimal Augmented
        {
            get => _decAug;
            set
            {
                if (_decAug != value)
                {
                    _decAug = value;
                    if (Enabled)
                    {
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                        this.ProcessRelevantEvents();
                    }
                }
            }
        }

        /// <summary>
        /// Value modifier.
        /// </summary>
        public decimal Value
        {
            get => _decVal;
            set
            {
                if (_decVal != value)
                {
                    _decVal = value;
                    if (Enabled)
                    {
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                        this.ProcessRelevantEvents();
                    }
                }
            }
        }

        /// <summary>
        /// The Rating value for the Improvement. This is 1 by default.
        /// </summary>
        public int Rating
        {
            get => _intRating;
            set
            {
                if (Interlocked.Exchange(ref _intRating, value) != value && Enabled)
                {
                    ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                    this.ProcessRelevantEvents();
                }
            }
        }

        /// <summary>
        /// A list of child items that should not receive the Improvement's benefit (typically for excluding a Skill from a Skill Group bonus).
        /// </summary>
        public string Exclude
        {
            get => _strExclude;
            set
            {
                if (Interlocked.Exchange(ref _strExclude, value) != value && Enabled)
                    this.ProcessRelevantEvents();
            }
        }

        /// <summary>
        /// String containing the condition for when the bonus applies (e.g. a dicepool bonus to a skill that only applies to certain types of tests).
        /// </summary>
        public string Condition
        {
            get => _strCondition;
            set
            {
                string strOldValue = Interlocked.Exchange(ref _strCondition, value);
                if (strOldValue != value && Enabled)
                {
                    if (string.IsNullOrEmpty(strOldValue) || string.IsNullOrEmpty(value))
                        ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                    this.ProcessRelevantEvents();
                }
            }
        }

        /// <summary>
        /// A Unique name for the Improvement. Only the highest value of any one Improvement that is part of this Unique Name group will be applied.
        /// </summary>
        public string UniqueName
        {
            get => _strUniqueName;
            set
            {
                string strOldValue = Interlocked.Exchange(ref _strUniqueName, value);
                if (strOldValue != value && Enabled)
                {
                    ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                    this.ProcessRelevantEvents(lstExtraUniqueName: strOldValue.Yield().ToList());
                }
            }
        }

        /// <summary>
        /// Whether or not the bonus applies directly to a Skill's Rating
        /// </summary>
        public bool AddToRating
        {
            get => _intAddToRating > 0;
            set
            {
                int intNewValue = value.ToInt32();
                if (Interlocked.Exchange(ref _intAddToRating, intNewValue) != intNewValue && Enabled)
                {
                    ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                    this.ProcessRelevantEvents();
                }
            }
        }

        /// <summary>
        /// The target of an improvement, e.g. the skill whose attributes should be swapped
        /// </summary>
        public string Target
        {
            get => _strTarget;
            set
            {
                string strOldValue = Interlocked.Exchange(ref _strTarget, value);
                if (strOldValue != value && Enabled)
                    this.ProcessRelevantEvents(lstExtraTarget: strOldValue.Yield().ToList());
            }
        }

        /// <summary>
        /// Whether or not the Improvement is enabled and provided its bonus.
        /// </summary>
        public bool Enabled
        {
            get => _intEnabled > 0;
            set
            {
                int intNewValue = value.ToInt32();
                if (Interlocked.Exchange(ref _intEnabled, intNewValue) == intNewValue)
                    return;
                ImprovementManager.ClearCachedValue(_objCharacter, ImproveType, ImprovedName);
                this.ProcessRelevantEvents();
            }
        }

        /// <summary>
        /// Whether or not we have completed our first setup. Needed to skip superfluous event updates at startup
        /// </summary>
        public bool SetupComplete
        {
            get => _blnSetupComplete;
            set => _blnSetupComplete = value;
        }

        /// <summary>
        /// Sort order for Custom Improvements.
        /// </summary>
        public int SortOrder
        {
            get => _intOrder;
            set => _intOrder = value;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Get an enumerable of events to fire related to this specific improvement.
        /// TODO: Merge parts or all of this function with ImprovementManager methods that enable, disable, add, or remove improvements.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Tuple<INotifyMultiplePropertyChanged, string>> GetRelevantPropertyChangers(ICollection<string> lstExtraImprovedName = null, ImprovementType eOverrideType = ImprovementType.None, ICollection<string> lstExtraUniqueName = null, ICollection<string> lstExtraTarget = null)
        {
            switch (eOverrideType != ImprovementType.None ? eOverrideType : ImproveType)
            {
                case ImprovementType.Attribute:
                {
                    string strTargetAttribute = ImprovedName;
                    using (new FetchSafelyFromPool<HashSet<string>>(Utils.StringHashSetPool,
                                                                    out HashSet<string>
                                                                        setAttributePropertiesChanged))
                    {
                        if (AugmentedMaximum != 0)
                            setAttributePropertiesChanged.Add(nameof(CharacterAttrib.AugmentedMaximumModifiers));
                        if (Maximum != 0)
                            setAttributePropertiesChanged.Add(nameof(CharacterAttrib.MaximumModifiers));
                        if (Minimum != 0)
                            setAttributePropertiesChanged.Add(nameof(CharacterAttrib.MinimumModifiers));
                        if (lstExtraImprovedName != null)
                        {
                            foreach (string strExtraAttribute in lstExtraImprovedName.Where(x => x.EndsWith("Base", StringComparison.Ordinal)).ToList())
                            {
                                lstExtraImprovedName.Add(strExtraAttribute.TrimEndOnce("Base", true));
                            }
                        }
                        strTargetAttribute = strTargetAttribute.TrimEndOnce("Base");
                        if (Augmented != 0)
                        {
                            setAttributePropertiesChanged.Add(nameof(CharacterAttrib.AttributeModifiers));
                            setAttributePropertiesChanged.Add(nameof(CharacterAttrib.HasModifiers));
                        }
                        else if ((ImproveSource == ImprovementSource.EssenceLoss ||
                                  ImproveSource == ImprovementSource.EssenceLossChargen ||
                                  ImproveSource == ImprovementSource.CyberadeptDaemon)
                                 && (_objCharacter.MAGEnabled && (strTargetAttribute == "MAG"
                                                                  || strTargetAttribute == "MAGAdept"
                                                                  || (lstExtraImprovedName != null
                                                                      && (lstExtraImprovedName.Contains("MAG")
                                                                          || lstExtraImprovedName
                                                                              .Contains("MAGAdept")))) ||
                                     _objCharacter.RESEnabled && (strTargetAttribute == "RES"
                                                                  || lstExtraImprovedName?.Contains("RES") == true) ||
                                     _objCharacter.DEPEnabled && (strTargetAttribute == "DEP"
                                                                  || lstExtraImprovedName?.Contains("DEP") == true)))
                            setAttributePropertiesChanged.Add(nameof(CharacterAttrib.HasModifiers));

                        if (setAttributePropertiesChanged.Count > 0)
                        {
                            foreach (CharacterAttrib objCharacterAttrib in _objCharacter.GetAllAttributes())
                            {
                                if (objCharacterAttrib.Abbrev != strTargetAttribute && lstExtraImprovedName?.Contains(objCharacterAttrib.Abbrev) != true)
                                    continue;
                                foreach (string strPropertyName in setAttributePropertiesChanged)
                                {
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(
                                        objCharacterAttrib,
                                        strPropertyName);
                                }
                            }
                        }
                    }
                }
                    break;

                case ImprovementType.AttributeMaxClamp:
                {
                    string strTargetAttribute = ImprovedName;
                    foreach (CharacterAttrib objCharacterAttrib in _objCharacter.GetAllAttributes())
                    {
                        if (objCharacterAttrib.Abbrev != strTargetAttribute && lstExtraImprovedName?.Contains(objCharacterAttrib.Abbrev) != true)
                            continue;
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(
                            objCharacterAttrib,
                            nameof(CharacterAttrib.AttributeModifiers));
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(
                            objCharacterAttrib,
                            nameof(CharacterAttrib.TotalAugmentedMaximum));
                    }
                }
                    break;

                case ImprovementType.Armor:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.GetArmorRating));
                }
                    break;

                case ImprovementType.FireArmor:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalFireArmorRating));
                }
                    break;

                case ImprovementType.ColdArmor:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalColdArmorRating));
                }
                    break;

                case ImprovementType.ElectricityArmor:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalElectricityArmorRating));
                }
                    break;

                case ImprovementType.AcidArmor:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalAcidArmorRating));
                }
                    break;

                case ImprovementType.FallingArmor:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalFallingArmorRating));
                }
                    break;

                case ImprovementType.Dodge:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalBonusDodgeRating));
                }
                    break;

                case ImprovementType.Reach:
                    break;

                case ImprovementType.Nuyen:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalStartingNuyen));
                }
                    break;

                case ImprovementType.PhysicalCM:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PhysicalCM));
                }
                    break;

                case ImprovementType.StunCM:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.StunCM));
                }
                    break;

                case ImprovementType.UnarmedDV:
                    break;

                case ImprovementType.InitiativeDiceAdd:
                case ImprovementType.InitiativeDice:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.InitiativeDice));
                }
                    break;

                case ImprovementType.MatrixInitiative:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.MatrixInitiativeValue));
                }
                    break;

                case ImprovementType.MatrixInitiativeDiceAdd:
                case ImprovementType.MatrixInitiativeDice:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.MatrixInitiativeDice));
                }
                    break;

                case ImprovementType.LifestyleCost:
                    break;

                case ImprovementType.CMThreshold:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CMThreshold));
                }
                    break;

                case ImprovementType.IgnoreCMPenaltyPhysical:
                case ImprovementType.IgnoreCMPenaltyStun:
                case ImprovementType.CMThresholdOffset:
                case ImprovementType.CMSharedThresholdOffset:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CMThresholdOffsets));
                }
                    break;

                case ImprovementType.EnhancedArticulation:
                    break;

                case ImprovementType.WeaponCategoryDV:
                    break;

                case ImprovementType.WeaponCategoryDice:
                    break;

                case ImprovementType.SpecialTab:
                    break;

                case ImprovementType.Initiative:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.InitiativeValue));
                }
                    break;

                case ImprovementType.LivingPersonaDeviceRating:
                    break;

                case ImprovementType.LivingPersonaProgramLimit:
                    break;

                case ImprovementType.LivingPersonaAttack:
                    break;

                case ImprovementType.LivingPersonaSleaze:
                    break;

                case ImprovementType.LivingPersonaDataProcessing:
                    break;

                case ImprovementType.LivingPersonaFirewall:
                    break;

                case ImprovementType.LivingPersonaMatrixCM:
                    break;

                case ImprovementType.Smartlink:
                    break;

                case ImprovementType.CyberwareEssCostNonRetroactive:
                case ImprovementType.CyberwareTotalEssMultiplierNonRetroactive:
                case ImprovementType.BiowareEssCostNonRetroactive:
                case ImprovementType.BiowareTotalEssMultiplierNonRetroactive:
                {
                    if (!_objCharacter.Created)
                    {
                        // Immediately reset cached essence to make sure this fires off before any other property changers would
                        _objCharacter.ResetCachedEssence();
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                            nameof(Character.Essence));
                    }
                    break;
                }
                case ImprovementType.GenetechCostMultiplier:
                    break;

                case ImprovementType.SoftWeave:
                    break;

                case ImprovementType.DisableBioware:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.AddBiowareEnabled));
                }
                    break;

                case ImprovementType.DisableCyberware:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.AddCyberwareEnabled));
                }
                    break;

                case ImprovementType.DisableBiowareGrade:
                    break;

                case ImprovementType.DisableCyberwareGrade:
                    break;

                case ImprovementType.ConditionMonitor:
                    break;

                case ImprovementType.UnarmedDVPhysical:
                    break;

                case ImprovementType.Adapsin:
                    break;

                case ImprovementType.FreePositiveQualities:
                    break;

                case ImprovementType.FreeNegativeQualities:
                    break;

                case ImprovementType.FreeKnowledgeSkills:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter.SkillsSection,
                        nameof(SkillsSection.KnowledgeSkillPoints));
                }
                    break;

                case ImprovementType.NuyenMaxBP:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalNuyenMaximumBP));
                }
                    break;

                case ImprovementType.CMOverflow:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CMOverflow));
                }
                    break;

                case ImprovementType.FreeSpiritPowerPoints:
                    break;

                case ImprovementType.AdeptPowerPoints:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PowerPointsTotal));
                }
                    break;

                case ImprovementType.ArmorEncumbrancePenalty:
                    break;

                case ImprovementType.Art:
                    break;

                case ImprovementType.Metamagic:
                    break;

                case ImprovementType.Echo:
                    break;

                case ImprovementType.Skillwire:
                {
                    foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objSkill,
                            nameof(Skill.CyberwareRating));
                    }
                }
                    break;

                case ImprovementType.DamageResistance:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.DamageResistancePool));
                }
                    break;

                case ImprovementType.JudgeIntentions:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.JudgeIntentions));
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.JudgeIntentionsResist));
                }
                    break;

                case ImprovementType.JudgeIntentionsOffense:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.JudgeIntentions));
                }
                    break;

                case ImprovementType.JudgeIntentionsDefense:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.JudgeIntentionsResist));
                }
                    break;

                case ImprovementType.LiftAndCarry:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.LiftAndCarry));
                }
                    break;

                case ImprovementType.Memory:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Memory));
                }
                    break;

                case ImprovementType.Concealability:
                    break;

                case ImprovementType.SwapSkillAttribute:
                case ImprovementType.SwapSkillSpecAttribute:
                {
                    if (lstExtraTarget?.Count > 0)
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == Target || lstExtraTarget.Contains(strKey))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.DefaultAttribute));
                            }
                        }

                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == Target || lstExtraTarget.Contains(strKey)
                                                 || Target == objTargetSkill.InternalId
                                                 || lstExtraTarget.Contains(objTargetSkill.InternalId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.DefaultAttribute));
                            }
                            else
                            {
                                string strDisplayName = objTargetSkill.CurrentDisplayName;
                                if (strDisplayName == Target || lstExtraTarget.Contains(strDisplayName))
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.DefaultAttribute));
                            }
                        }
                    }
                    else
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == Target)
                            ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                x.InternalId == Target || x.DictionaryKey == Target
                                                       || x.CurrentDisplayName == Target);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.DefaultAttribute));
                        }
                    }
                }
                    break;

                case ImprovementType.DrainResistance:
                case ImprovementType.FadingResistance:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter.MagicTradition,
                        nameof(Tradition.DrainValue));
                }
                    break;

                case ImprovementType.Composure:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Composure));
                }
                    break;

                case ImprovementType.UnarmedAP:
                    break;

                case ImprovementType.Restricted:
                    break;

                case ImprovementType.Notoriety:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CalculatedNotoriety));
                }
                    break;

                case ImprovementType.SpellCategory:
                    break;

                case ImprovementType.SpellCategoryDamage:
                    break;

                case ImprovementType.SpellCategoryDrain:
                    break;

                case ImprovementType.ThrowRange:
                    break;

                case ImprovementType.SkillsoftAccess:
                {
                    // Keeping two enumerations separate helps avoid extra heap allocations
                    foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objSkill,
                            nameof(Skill.CyberwareRating));
                    }

                    foreach (KnowledgeSkill objSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objSkill,
                            nameof(Skill.CyberwareRating));
                    }
                }
                    break;

                case ImprovementType.AddSprite:
                    break;

                case ImprovementType.BlackMarketDiscount:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.BlackMarketDiscount));
                }
                    break;

                case ImprovementType.ComplexFormLimit:
                    break;

                case ImprovementType.SpellLimit:
                    break;

                case ImprovementType.QuickeningMetamagic:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.QuickeningEnabled));
                }
                    break;

                case ImprovementType.BasicLifestyleCost:
                    break;

                case ImprovementType.ThrowSTR:
                    break;

                case ImprovementType.EssenceMax:
                {
                    foreach (CharacterAttrib objCharacterAttrib in _objCharacter.GetAllAttributes())
                    {
                        if (objCharacterAttrib.Abbrev == "ESS")
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                nameof(CharacterAttrib.MetatypeMaximum));
                        }
                    }
                }
                    break;

                case ImprovementType.AdeptPower:
                    break;

                case ImprovementType.SpecificQuality:
                    break;

                case ImprovementType.MartialArt:
                    break;

                case ImprovementType.LimitModifier:
                    break;

                case ImprovementType.PhysicalLimit:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.LimitPhysical));
                }
                    break;

                case ImprovementType.MentalLimit:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.LimitMental));
                }
                    break;

                case ImprovementType.SocialLimit:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.LimitSocial));
                }
                    break;

                case ImprovementType.FriendsInHighPlaces:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.FriendsInHighPlaces));
                }
                    break;

                case ImprovementType.Erased:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Erased));
                }
                    break;

                case ImprovementType.Fame:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Fame));
                }
                    break;

                case ImprovementType.MadeMan:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.MadeMan));
                }
                    break;

                case ImprovementType.Overclocker:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Overclocker));
                }
                    break;

                case ImprovementType.RestrictedGear:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.RestrictedGear));
                }
                    break;

                case ImprovementType.TrustFund:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TrustFund));
                }
                    break;

                case ImprovementType.ExCon:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.ExCon));
                }
                    break;

                case ImprovementType.ContactForceGroup:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Contact objTargetContact in _objCharacter.Contacts)
                        {
                            if (objTargetContact.UniqueId == ImprovedName
                                || lstExtraImprovedName.Contains(objTargetContact.UniqueId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetContact,
                                    nameof(Contact.GroupEnabled));
                            }
                        }
                    }
                    else
                    {
                        Contact objTargetContact
                            = _objCharacter.Contacts.FirstOrDefault(x => x.UniqueId == ImprovedName);
                        if (objTargetContact != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetContact,
                                nameof(Contact.GroupEnabled));
                        }
                    }
                }
                    break;

                case ImprovementType.Attributelevel:
                {
                    foreach (CharacterAttrib objCharacterAttrib in _objCharacter.GetAllAttributes())
                    {
                        if (objCharacterAttrib.Abbrev == ImprovedName || lstExtraImprovedName?.Contains(objCharacterAttrib.Abbrev) == true)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                nameof(CharacterAttrib.FreeBase));
                        }
                    }
                }
                    break;

                case ImprovementType.AddContact:
                    break;

                case ImprovementType.Seeker:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.RedlinerBonus));
                }
                    break;

                case ImprovementType.PublicAwareness:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CalculatedPublicAwareness));
                }
                    break;

                case ImprovementType.PrototypeTranshuman:
                    break;

                case ImprovementType.Hardwire:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.CyberwareRating));
                            }
                        }

                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey)
                                                       || ImprovedName == objTargetSkill.InternalId
                                                       || lstExtraImprovedName.Contains(objTargetSkill.InternalId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.CyberwareRating));
                            }
                            else
                            {
                                string strDisplayName = objTargetSkill.CurrentDisplayName;
                                if (strDisplayName == ImprovedName || lstExtraImprovedName.Contains(strDisplayName))
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.CyberwareRating));
                            }
                        }
                    }
                    else
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName)
                            ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                x.InternalId == ImprovedName || x.DictionaryKey == ImprovedName
                                                             || x.CurrentDisplayName == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.CyberwareRating));
                        }
                    }
                }
                    break;

                case ImprovementType.DealerConnection:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.DealerConnectionDiscount));
                }
                    break;

                case ImprovementType.BlockSkillDefault:
                case ImprovementType.AllowSkillDefault:
                {
                    if (string.IsNullOrEmpty(ImprovedName))
                    {
                        // Kludgiest of kludges, but it fits spec and Sapience isn't exactly getting turned off and on constantly.
                        foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objSkill,
                                nameof(Skill.Default));
                        }

                        foreach (KnowledgeSkill objSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objSkill,
                                nameof(Skill.Default));
                        }
                    }
                    else if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.Default));
                            }
                        }

                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey)
                                                       || ImprovedName == objTargetSkill.InternalId
                                                       || lstExtraImprovedName.Contains(objTargetSkill.InternalId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.Default));
                            }
                            else
                            {
                                string strDisplayName = objTargetSkill.CurrentDisplayName;
                                if (strDisplayName == ImprovedName || lstExtraImprovedName.Contains(strDisplayName))
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.Default));
                            }
                        }
                    }
                    else
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName)
                            ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                x.InternalId == ImprovedName || x.DictionaryKey == ImprovedName
                                                             || x.CurrentDisplayName == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.Default));
                        }
                    }
                }
                    break;

                case ImprovementType.Skill:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.Base));
                            }
                        }

                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey)
                                                       || ImprovedName == objTargetSkill.InternalId
                                                       || lstExtraImprovedName.Contains(objTargetSkill.InternalId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.Base));
                            }
                            else
                            {
                                string strDisplayName = objTargetSkill.CurrentDisplayName;
                                if (strDisplayName == ImprovedName || lstExtraImprovedName.Contains(strDisplayName))
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.Base));
                            }
                        }
                    }
                    else
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName)
                            ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                x.InternalId == ImprovedName || x.DictionaryKey == ImprovedName
                                                             || x.CurrentDisplayName == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.Base));
                        }
                    }
                }
                    break;

                case ImprovementType.SkillGroup:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                    {
                        if (objTargetSkill.SkillGroup == ImprovedName || lstExtraImprovedName?.Contains(objTargetSkill.SkillGroup) == true)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.PoolModifiers));
                    }
                }
                    break;

                case ImprovementType.BlockSkillGroupDefault:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                    {
                        if (objTargetSkill.SkillGroup == ImprovedName || lstExtraImprovedName?.Contains(objTargetSkill.SkillGroup) == true)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.Default));
                    }
                }
                    break;

                case ImprovementType.SkillCategory:
                {
                    // Keeping two enumerations separate helps avoid extra heap allocations
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                    {
                        if (objTargetSkill.SkillCategory == ImprovedName || lstExtraImprovedName?.Contains(objTargetSkill.SkillCategory) == true)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.PoolModifiers));
                    }

                    foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                    {
                        if (objTargetSkill.SkillCategory == ImprovedName || lstExtraImprovedName?.Contains(objTargetSkill.SkillCategory) == true)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.PoolModifiers));
                    }
                }
                    break;

                case ImprovementType.BlockSkillCategoryDefault:
                {
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                    {
                        if (objTargetSkill.SkillCategory == ImprovedName || lstExtraImprovedName?.Contains(objTargetSkill.SkillCategory) == true)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.Default));
                    }
                }
                    break;

                case ImprovementType.SkillLinkedAttribute:
                {
                    // Keeping two enumerations separate helps avoid extra heap allocations
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                    {
                        if (objTargetSkill.Attribute == ImprovedName || lstExtraImprovedName?.Contains(objTargetSkill.Attribute) == true)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.PoolModifiers));
                    }

                    foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                    {
                        if (objTargetSkill.Attribute == ImprovedName || lstExtraImprovedName?.Contains(objTargetSkill.Attribute) == true)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.PoolModifiers));
                    }
                }
                    break;

                case ImprovementType.SkillLevel:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.FreeKarma));
                            }
                        }

                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey)
                                                       || ImprovedName == objTargetSkill.InternalId
                                                       || lstExtraImprovedName.Contains(objTargetSkill.InternalId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.FreeKarma));
                            }
                            else
                            {
                                string strDisplayName = objTargetSkill.CurrentDisplayName;
                                if (strDisplayName == ImprovedName || lstExtraImprovedName.Contains(strDisplayName))
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.FreeKarma));
                            }
                        }
                    }
                    else
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName)
                            ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                x.InternalId == ImprovedName || x.DictionaryKey == ImprovedName
                                                             || x.CurrentDisplayName == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.FreeKarma));
                        }
                    }
                }
                    break;

                case ImprovementType.SkillGroupLevel:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups)
                        {
                            if (objTargetGroup.Name == ImprovedName || lstExtraImprovedName.Contains(objTargetGroup.Name))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                                    nameof(SkillGroup.FreeLevels));
                            }
                        }
                    }
                    else
                    {
                        SkillGroup objTargetGroup =
                            _objCharacter.SkillsSection.SkillGroups.FirstOrDefault(x => x.Name == ImprovedName);
                        if (objTargetGroup != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                                nameof(SkillGroup.FreeLevels));
                        }
                    }
                }
                    break;

                case ImprovementType.SkillBase:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.FreeBase));
                            }
                        }

                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey)
                                                       || ImprovedName == objTargetSkill.InternalId
                                                       || lstExtraImprovedName.Contains(objTargetSkill.InternalId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.FreeBase));
                            }
                            else
                            {
                                string strDisplayName = objTargetSkill.CurrentDisplayName;
                                if (strDisplayName == ImprovedName || lstExtraImprovedName.Contains(strDisplayName))
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.FreeBase));
                            }
                        }
                    }
                    else
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName)
                            ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                x.InternalId == ImprovedName || x.DictionaryKey == ImprovedName
                                                             || x.CurrentDisplayName == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.FreeBase));
                        }
                    }
                }
                    break;

                case ImprovementType.SkillGroupBase:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups)
                        {
                            if (objTargetGroup.Name == ImprovedName || lstExtraImprovedName.Contains(objTargetGroup.Name))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                                    nameof(SkillGroup.FreeBase));
                            }
                        }
                    }
                    else
                    {
                        SkillGroup objTargetGroup =
                            _objCharacter.SkillsSection.SkillGroups.FirstOrDefault(x => x.Name == ImprovedName);
                        if (objTargetGroup != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                                nameof(SkillGroup.FreeBase));
                        }
                    }
                }
                    break;

                case ImprovementType.Skillsoft:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey)
                                                       || ImprovedName == objTargetSkill.InternalId
                                                       || lstExtraImprovedName.Contains(objTargetSkill.InternalId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.CyberwareRating));
                            }
                            else
                            {
                                string strDisplayName = objTargetSkill.CurrentDisplayName;
                                if (strDisplayName == ImprovedName || lstExtraImprovedName.Contains(strDisplayName))
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.CyberwareRating));
                            }
                        }
                    }
                    else
                    {
                        KnowledgeSkill objTargetSkill = _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                            x.InternalId == ImprovedName || x.DictionaryKey == ImprovedName
                                                         || x.CurrentDisplayName == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.CyberwareRating));
                        }
                    }
                }
                    break;

                case ImprovementType.Activesoft:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.CyberwareRating));
                            }
                        }
                    }
                    else
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.CyberwareRating));
                        }
                    }
                }
                    break;

                case ImprovementType.ReplaceAttribute:
                {
                    foreach (CharacterAttrib objCharacterAttrib in _objCharacter.GetAllAttributes())
                    {
                        if ((objCharacterAttrib.Abbrev != ImprovedName && lstExtraImprovedName?.Contains(objCharacterAttrib.Abbrev) != true)
                            || objCharacterAttrib.MetatypeCategory == CharacterAttrib.AttributeCategory.Shapeshifter)
                            continue;
                        if (Maximum != 0)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                nameof(CharacterAttrib.MetatypeMaximum));
                        if (Minimum != 0)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                nameof(CharacterAttrib.MetatypeMinimum));
                        if (AugmentedMaximum != 0)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                nameof(CharacterAttrib.MetatypeAugmentedMaximum));
                    }
                }
                    break;

                case ImprovementType.SpecialSkills:
                    break;

                case ImprovementType.SkillAttribute:
                case ImprovementType.ReflexRecorderOptimization:
                {
                    foreach (Skill objSkill in _objCharacter.SkillsSection.Skills)
                    {
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objSkill,
                            nameof(Skill.PoolModifiers));
                    }
                }
                    break;

                case ImprovementType.Ambidextrous:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Ambidextrous));
                }
                    break;

                case ImprovementType.UnarmedReach:
                    break;

                case ImprovementType.SkillExpertise:
                case ImprovementType.SkillSpecialization:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.Specializations));
                            }
                        }

                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey)
                                                       || ImprovedName == objTargetSkill.InternalId
                                                       || lstExtraImprovedName.Contains(objTargetSkill.InternalId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.Specializations));
                            }
                            else
                            {
                                string strDisplayName = objTargetSkill.CurrentDisplayName;
                                if (strDisplayName == ImprovedName || lstExtraImprovedName.Contains(strDisplayName))
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.Specializations));
                            }
                        }
                    }
                    else
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName)
                            ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                x.InternalId == ImprovedName || x.DictionaryKey == ImprovedName
                                                             || x.CurrentDisplayName == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.Specializations));
                        }
                    }

                    break;
                }

                case ImprovementType.SkillSpecializationOption:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.CGLSpecializations));
                            }
                        }

                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey)
                                                       || ImprovedName == objTargetSkill.InternalId
                                                       || lstExtraImprovedName.Contains(objTargetSkill.InternalId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.CGLSpecializations));
                            }
                            else
                            {
                                string strDisplayName = objTargetSkill.CurrentDisplayName;
                                if (strDisplayName == ImprovedName || lstExtraImprovedName.Contains(strDisplayName))
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.CGLSpecializations));
                            }
                        }
                    }
                    else
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName)
                            ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                x.InternalId == ImprovedName || x.DictionaryKey == ImprovedName
                                                             || x.CurrentDisplayName == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.CGLSpecializations));
                        }
                    }

                    break;
                }
                case ImprovementType.NativeLanguageLimit:
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter.SkillsSection,
                        nameof(SkillsSection.HasAvailableNativeLanguageSlots));
                    break;

                case ImprovementType.AdeptPowerFreePoints:
                {
                    // Get the power improved by this improvement
                    if (lstExtraImprovedName?.Count > 0 || lstExtraUniqueName?.Count > 0)
                    {
                        foreach (Power objImprovedPower in _objCharacter.Powers)
                        {
                            if ((objImprovedPower.Name == ImprovedName || lstExtraImprovedName?.Contains(objImprovedPower.Name) == true)
                                && (objImprovedPower.Extra == UniqueName || lstExtraUniqueName?.Contains(objImprovedPower.Extra) == true))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objImprovedPower,
                                    nameof(Power.FreePoints));
                            }
                        }
                    }
                    else
                    {
                        Power objImprovedPower = _objCharacter.Powers.FirstOrDefault(objPower =>
                            objPower.Name == ImprovedName && objPower.Extra == UniqueName);
                        if (objImprovedPower != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objImprovedPower,
                                nameof(Power.FreePoints));
                        }
                    }
                }
                    break;

                case ImprovementType.AdeptPowerFreeLevels:
                {
                    // Get the power improved by this improvement
                    if (lstExtraImprovedName?.Count > 0 || lstExtraUniqueName?.Count > 0)
                    {
                        foreach (Power objImprovedPower in _objCharacter.Powers)
                        {
                            if ((objImprovedPower.Name == ImprovedName
                                 || lstExtraImprovedName?.Contains(objImprovedPower.Name) == true)
                                && (objImprovedPower.Extra == UniqueName
                                    || lstExtraUniqueName?.Contains(objImprovedPower.Extra) == true))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objImprovedPower,
                                    nameof(Power.FreeLevels));
                            }
                        }
                    }
                    else
                    {
                        Power objImprovedPower = _objCharacter.Powers.FirstOrDefault(objPower =>
                            objPower.Name == ImprovedName && objPower.Extra == UniqueName);
                        if (objImprovedPower != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objImprovedPower,
                                nameof(Power.FreeLevels));
                        }
                    }
                }
                    break;

                case ImprovementType.AIProgram:
                    break;

                case ImprovementType.CritterPowerLevel:
                    break;

                case ImprovementType.CritterPower:
                    break;

                case ImprovementType.SpellResistance:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellResistance));
                }
                    break;

                case ImprovementType.LimitSpellCategory:
                    break;

                case ImprovementType.LimitSpellDescriptor:
                    break;

                case ImprovementType.LimitSpiritCategory:
                    break;

                case ImprovementType.WalkSpeed:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.WalkingRate));
                }
                    break;

                case ImprovementType.RunSpeed:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.RunningRate));
                }
                    break;

                case ImprovementType.SprintSpeed:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SprintingRate));
                }
                    break;

                case ImprovementType.WalkMultiplier:
                case ImprovementType.WalkMultiplierPercent:
                case ImprovementType.RunMultiplier:
                case ImprovementType.RunMultiplierPercent:
                case ImprovementType.SprintBonus:
                case ImprovementType.SprintBonusPercent:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CalculatedMovement));
                }
                    break;

                case ImprovementType.EssencePenalty:
                case ImprovementType.EssencePenaltyT100:
                case ImprovementType.EssencePenaltyMAGOnlyT100:
                case ImprovementType.EssencePenaltyRESOnlyT100:
                case ImprovementType.EssencePenaltyDEPOnlyT100:
                case ImprovementType.SpecialAttBurn:
                case ImprovementType.SpecialAttTotalBurnMultiplier:
                case ImprovementType.CyborgEssence:
                case ImprovementType.CyberwareEssCost:
                case ImprovementType.CyberwareTotalEssMultiplier:
                case ImprovementType.BiowareEssCost:
                case ImprovementType.BiowareTotalEssMultiplier:
                case ImprovementType.BasicBiowareEssCost:
                case ImprovementType.GenetechEssMultiplier:
                    // Immediately reset cached essence to make sure this fires off before any other property changers would
                    _objCharacter.ResetCachedEssence();
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Essence));
                    break;

                case ImprovementType.FreeSpellsATT:
                    break;

                case ImprovementType.FreeSpells:
                    break;

                case ImprovementType.DrainValue:
                    break;

                case ImprovementType.FadingValue:
                    break;

                case ImprovementType.Spell:
                    break;

                case ImprovementType.ComplexForm:
                    break;

                case ImprovementType.Gear:
                    break;

                case ImprovementType.Weapon:
                    break;

                case ImprovementType.MentorSpirit:
                    break;

                case ImprovementType.Paragon:
                    break;

                case ImprovementType.FreeSpellsSkill:
                    break;

                case ImprovementType.DisableSpecializationEffects:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.GetSpecializationBonus));
                            }
                        }

                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey)
                                                       || ImprovedName == objTargetSkill.InternalId
                                                       || lstExtraImprovedName.Contains(objTargetSkill.InternalId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.GetSpecializationBonus));
                            }
                            else
                            {
                                string strDisplayName = objTargetSkill.CurrentDisplayName;
                                if (strDisplayName == ImprovedName || lstExtraImprovedName.Contains(strDisplayName))
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.GetSpecializationBonus));
                            }
                        }
                    }
                    else
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName)
                            ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                x.InternalId == ImprovedName || x.DictionaryKey == ImprovedName
                                                             || x.CurrentDisplayName == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.GetSpecializationBonus));
                        }
                    }
                }
                    break;

                case ImprovementType.PhysiologicalAddictionFirstTime:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PhysiologicalAddictionResistFirstTime));
                }
                    break;

                case ImprovementType.PsychologicalAddictionFirstTime:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PsychologicalAddictionResistFirstTime));
                }
                    break;

                case ImprovementType.PhysiologicalAddictionAlreadyAddicted:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PhysiologicalAddictionResistAlreadyAddicted));
                }
                    break;

                case ImprovementType.PsychologicalAddictionAlreadyAddicted:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PsychologicalAddictionResistAlreadyAddicted));
                }
                    break;

                case ImprovementType.AddESStoStunCMRecovery:
                case ImprovementType.StunCMRecovery:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.StunCMNaturalRecovery));
                }
                    break;

                case ImprovementType.AddESStoPhysicalCMRecovery:
                case ImprovementType.PhysicalCMRecovery:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.PhysicalCMNaturalRecovery));
                }
                    break;

                case ImprovementType.MentalManipulationResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseManipulationMental));
                }
                    break;

                case ImprovementType.PhysicalManipulationResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseManipulationPhysical));
                }
                    break;

                case ImprovementType.ManaIllusionResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseIllusionMana));
                }
                    break;

                case ImprovementType.PhysicalIllusionResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseIllusionPhysical));
                }
                    break;

                case ImprovementType.DetectionSpellResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDetection));
                }
                    break;

                case ImprovementType.DirectManaSpellResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDirectSoakMana));
                }
                    break;

                case ImprovementType.DirectPhysicalSpellResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDirectSoakPhysical));
                }
                    break;

                case ImprovementType.DecreaseBODResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseBOD));
                }
                    break;

                case ImprovementType.DecreaseAGIResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseAGI));
                }
                    break;

                case ImprovementType.DecreaseREAResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseREA));
                }
                    break;

                case ImprovementType.DecreaseSTRResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseSTR));
                }
                    break;

                case ImprovementType.DecreaseCHAResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseCHA));
                }
                    break;

                case ImprovementType.DecreaseINTResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseINT));
                }
                    break;

                case ImprovementType.DecreaseLOGResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseLOG));
                }
                    break;

                case ImprovementType.DecreaseWILResist:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellDefenseDecreaseWIL));
                }
                    break;

                case ImprovementType.AddLimb:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.LimbCount));
                }
                    break;

                case ImprovementType.StreetCredMultiplier:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.CalculatedStreetCred));
                }
                    break;

                case ImprovementType.StreetCred:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.TotalStreetCred));
                }
                    break;

                case ImprovementType.AttributeKarmaCostMultiplier:
                case ImprovementType.AttributeKarmaCost:
                {
                    foreach (CharacterAttrib objCharacterAttrib in _objCharacter.GetAllAttributes())
                    {
                        if (string.IsNullOrEmpty(ImprovedName) || objCharacterAttrib.Abbrev == ImprovedName || lstExtraImprovedName?.Contains(objCharacterAttrib.Abbrev) == true)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objCharacterAttrib,
                                nameof(CharacterAttrib.UpgradeKarmaCost));
                        }
                    }
                }
                    break;

                case ImprovementType.ActiveSkillKarmaCost:
                case ImprovementType.ActiveSkillKarmaCostMultiplier:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        if (lstExtraImprovedName?.Count > 0)
                        {
                            foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                            {
                                string strKey = objTargetSkill.DictionaryKey;
                                if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey))
                                {
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.UpgradeKarmaCost));
                                }
                            }
                        }
                        else
                        {
                            Skill objTargetSkill =
                                _objCharacter.SkillsSection.Skills.FirstOrDefault(
                                    x => x.DictionaryKey == ImprovedName);
                            if (objTargetSkill != null)
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.UpgradeKarmaCost));
                            }
                        }
                    }
                    else
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.UpgradeKarmaCost));
                        }
                    }
                }
                    break;

                case ImprovementType.KnowledgeSkillKarmaCost:
                case ImprovementType.KnowledgeSkillKarmaCostMinimum:
                case ImprovementType.KnowledgeSkillKarmaCostMultiplier:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        if (lstExtraImprovedName?.Count > 0)
                        {
                            foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                            {
                                string strKey = objTargetSkill.DictionaryKey;
                                if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey)
                                                           || ImprovedName == objTargetSkill.InternalId
                                                           || lstExtraImprovedName.Contains(objTargetSkill.InternalId))
                                {
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.UpgradeKarmaCost));
                                }
                                else
                                {
                                    string strDisplayName = objTargetSkill.CurrentDisplayName;
                                    if (strDisplayName == ImprovedName || lstExtraImprovedName.Contains(strDisplayName))
                                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                            nameof(Skill.UpgradeKarmaCost));
                                }
                            }
                        }
                        else
                        {
                            KnowledgeSkill objTargetSkill
                                = _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                    x.DictionaryKey == ImprovedName || x.CurrentDisplayName == ImprovedName);
                            if (objTargetSkill != null)
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.UpgradeKarmaCost));
                            }
                        }
                    }
                    else
                    {
                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.UpgradeKarmaCost));
                        }
                    }
                }
                    break;

                case ImprovementType.SkillGroupKarmaCost:
                case ImprovementType.SkillGroupKarmaCostMultiplier:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        if (lstExtraImprovedName?.Count > 0)
                        {
                            foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups)
                            {
                                if (objTargetGroup.Name == ImprovedName || lstExtraImprovedName.Contains(objTargetGroup.Name))
                                {
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                                        nameof(SkillGroup.UpgradeKarmaCost));
                                }
                            }
                        }
                        else
                        {
                            SkillGroup objTargetGroup =
                                _objCharacter.SkillsSection.SkillGroups.FirstOrDefault(x => x.Name == ImprovedName);
                            if (objTargetGroup != null)
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                                    nameof(SkillGroup.UpgradeKarmaCost));
                            }
                        }
                    }
                    else
                    {
                        foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                                nameof(SkillGroup.UpgradeKarmaCost));
                        }
                    }
                }
                    break;

                case ImprovementType.SkillGroupDisable:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups)
                        {
                            if (objTargetGroup.Name == ImprovedName || lstExtraImprovedName.Contains(objTargetGroup.Name))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                                    nameof(SkillGroup.IsDisabled));
                            }
                        }
                    }
                    else
                    {
                        SkillGroup objTargetGroup =
                            _objCharacter.SkillsSection.SkillGroups.FirstOrDefault(x => x.Name == ImprovedName);
                        if (objTargetGroup != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetGroup,
                                nameof(SkillGroup.IsDisabled));
                        }
                    }

                    break;
                }
                case ImprovementType.SkillDisable:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.Enabled));
                            }
                        }

                        foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                        {
                            string strKey = objTargetSkill.DictionaryKey;
                            if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey)
                                                       || ImprovedName == objTargetSkill.InternalId
                                                       || lstExtraImprovedName.Contains(objTargetSkill.InternalId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.Enabled));
                            }
                            else
                            {
                                string strDisplayName = objTargetSkill.CurrentDisplayName;
                                if (strDisplayName == ImprovedName || lstExtraImprovedName.Contains(strDisplayName))
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.Enabled));
                            }
                        }
                    }
                    else
                    {
                        Skill objTargetSkill =
                            _objCharacter.SkillsSection.Skills.FirstOrDefault(x => x.DictionaryKey == ImprovedName)
                            ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                x.InternalId == ImprovedName || x.DictionaryKey == ImprovedName
                                                             || x.CurrentDisplayName == ImprovedName);
                        if (objTargetSkill != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.Enabled));
                        }
                    }
                }
                    break;

                case ImprovementType.SkillCategorySpecializationKarmaCost:
                case ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                {
                    // Keeping two enumerations separate helps avoid extra heap allocations
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                    {
                        if (objTargetSkill.SkillCategory == ImprovedName || lstExtraImprovedName?.Contains(objTargetSkill.SkillCategory) == true)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.CanAffordSpecialization));
                    }

                    foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                    {
                        if (objTargetSkill.SkillCategory == ImprovedName || lstExtraImprovedName?.Contains(objTargetSkill.SkillCategory) == true)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.CanAffordSpecialization));
                    }
                }
                    break;

                case ImprovementType.SkillCategoryKarmaCost:
                case ImprovementType.SkillCategoryKarmaCostMultiplier:
                {
                    // Keeping two enumerations separate helps avoid extra heap allocations
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                    {
                        if (objTargetSkill.SkillCategory == ImprovedName || lstExtraImprovedName?.Contains(objTargetSkill.SkillCategory) == true)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.UpgradeKarmaCost));
                    }

                    foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                    {
                        if (objTargetSkill.SkillCategory == ImprovedName || lstExtraImprovedName?.Contains(objTargetSkill.SkillCategory) == true)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.UpgradeKarmaCost));
                    }
                }
                    break;

                case ImprovementType.SkillGroupCategoryDisable:
                {
                    foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups)
                    {
                        if (objTargetGroup.GetRelevantSkillCategories.Contains(ImprovedName)
                            || (lstExtraImprovedName != null
                                && objTargetGroup.GetRelevantSkillCategories.Any(
                                    lstExtraImprovedName.Contains)))
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(
                                objTargetGroup, nameof(SkillGroup.IsDisabled));
                        }
                    }
                }
                    break;

                case ImprovementType.SkillGroupCategoryKarmaCostMultiplier:
                case ImprovementType.SkillGroupCategoryKarmaCost:
                {
                    foreach (SkillGroup objTargetGroup in _objCharacter.SkillsSection.SkillGroups)
                    {
                        if (objTargetGroup.GetRelevantSkillCategories.Contains(ImprovedName)
                            || (lstExtraImprovedName != null
                                && objTargetGroup.GetRelevantSkillCategories.Any(
                                    lstExtraImprovedName.Contains)))
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(
                                objTargetGroup, nameof(SkillGroup.UpgradeKarmaCost));
                        }
                    }
                }
                    break;

                case ImprovementType.AttributePointCost:
                case ImprovementType.AttributePointCostMultiplier:
                    break;

                case ImprovementType.ActiveSkillPointCost:
                case ImprovementType.ActiveSkillPointCostMultiplier:
                    break;

                case ImprovementType.SkillGroupPointCost:
                case ImprovementType.SkillGroupPointCostMultiplier:
                    break;

                case ImprovementType.KnowledgeSkillPointCost:
                case ImprovementType.KnowledgeSkillPointCostMultiplier:
                    break;

                case ImprovementType.SkillCategoryPointCost:
                case ImprovementType.SkillCategoryPointCostMultiplier:
                    break;

                case ImprovementType.SkillGroupCategoryPointCost:
                case ImprovementType.SkillGroupCategoryPointCostMultiplier:
                    break;

                case ImprovementType.NewSpellKarmaCost:
                case ImprovementType.NewSpellKarmaCostMultiplier:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpellKarmaCost));
                }
                    break;

                case ImprovementType.NewComplexFormKarmaCost:
                case ImprovementType.NewComplexFormKarmaCostMultiplier:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.ComplexFormKarmaCost));
                }
                    break;

                case ImprovementType.NewAIProgramKarmaCost:
                case ImprovementType.NewAIProgramKarmaCostMultiplier:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.AIProgramKarmaCost));
                }
                    break;

                case ImprovementType.NewAIAdvancedProgramKarmaCost:
                case ImprovementType.NewAIAdvancedProgramKarmaCostMultiplier:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.AIAdvancedProgramKarmaCost));
                }
                    break;

                case ImprovementType.BlockSkillSpecializations:
                {
                    if (!string.IsNullOrEmpty(ImprovedName))
                    {
                        if (lstExtraImprovedName?.Count > 0)
                        {
                            foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                            {
                                string strKey = objTargetSkill.DictionaryKey;
                                if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey))
                                {
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.CanHaveSpecs));
                                }
                            }

                            foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                            {
                                string strKey = objTargetSkill.DictionaryKey;
                                if (strKey == ImprovedName || lstExtraImprovedName.Contains(strKey)
                                                           || ImprovedName == objTargetSkill.InternalId
                                                           || lstExtraImprovedName.Contains(objTargetSkill.InternalId))
                                {
                                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                        nameof(Skill.CanHaveSpecs));
                                }
                                else
                                {
                                    string strDisplayName = objTargetSkill.CurrentDisplayName;
                                    if (strDisplayName == ImprovedName || lstExtraImprovedName.Contains(strDisplayName))
                                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                            nameof(Skill.CanHaveSpecs));
                                }
                            }
                        }
                        else
                        {
                            Skill objTargetSkill =
                                _objCharacter.SkillsSection.Skills.FirstOrDefault(
                                    x => x.DictionaryKey == ImprovedName)
                                ?? _objCharacter.SkillsSection.KnowledgeSkills.FirstOrDefault(x =>
                                    x.InternalId == ImprovedName || x.DictionaryKey == ImprovedName
                                                                 || x.CurrentDisplayName == ImprovedName);
                            if (objTargetSkill != null)
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                    nameof(Skill.CanHaveSpecs));
                            }
                        }
                    }
                    else
                    {
                        foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.CanHaveSpecs));
                        }
                    }
                }
                    break;

                case ImprovementType.BlockSkillCategorySpecializations:
                {
                    // Keeping two enumerations separate helps avoid extra heap allocations
                    foreach (Skill objTargetSkill in _objCharacter.SkillsSection.Skills)
                    {
                        if (objTargetSkill.SkillCategory == ImprovedName || lstExtraImprovedName?.Contains(objTargetSkill.SkillCategory) == true)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.CanHaveSpecs));
                    }

                    foreach (KnowledgeSkill objTargetSkill in _objCharacter.SkillsSection.KnowledgeSkills)
                    {
                        if (objTargetSkill.SkillCategory == ImprovedName || lstExtraImprovedName?.Contains(objTargetSkill.SkillCategory) == true)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetSkill,
                                nameof(Skill.CanHaveSpecs));
                    }
                }
                    break;

                case ImprovementType.FocusBindingKarmaCost:
                    break;

                case ImprovementType.FocusBindingKarmaMultiplier:
                    break;

                case ImprovementType.MagiciansWayDiscount:
                {
                    foreach (Power objLoopPower in _objCharacter.Powers)
                    {
                        if (objLoopPower.AdeptWayDiscount != 0)
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objLoopPower,
                                nameof(Power.AdeptWayDiscountEnabled));
                    }
                }
                    break;

                case ImprovementType.BurnoutsWay:
                    break;

                case ImprovementType.ContactForcedLoyalty:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Contact objTargetContact in _objCharacter.Contacts)
                        {
                            if (objTargetContact.UniqueId == ImprovedName || lstExtraImprovedName.Contains(objTargetContact.UniqueId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetContact,
                                    nameof(Contact.ForcedLoyalty));
                            }
                        }
                    }
                    else
                    {
                        Contact objTargetContact = _objCharacter.Contacts.FirstOrDefault(x => x.UniqueId == ImprovedName);
                        if (objTargetContact != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetContact,
                                nameof(Contact.ForcedLoyalty));
                        }
                    }
                }
                    break;

                case ImprovementType.ContactMakeFree:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Contact objTargetContact in _objCharacter.Contacts)
                        {
                            if (objTargetContact.UniqueId == ImprovedName
                                || lstExtraImprovedName.Contains(objTargetContact.UniqueId))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetContact,
                                    nameof(Contact.Free));
                            }
                        }
                    }
                    else
                    {
                        Contact objTargetContact
                            = _objCharacter.Contacts.FirstOrDefault(x => x.UniqueId == ImprovedName);
                        if (objTargetContact != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objTargetContact,
                                nameof(Contact.Free));
                        }
                    }
                }
                    break;

                case ImprovementType.FreeWare:
                    break;

                case ImprovementType.WeaponSkillAccuracy:
                    break;

                case ImprovementType.WeaponAccuracy:
                    break;

                case ImprovementType.SpecialModificationLimit:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SpecialModificationLimit));
                }
                    break;

                case ImprovementType.MetageneticLimit:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.MetagenicLimit));
                }
                    break;

                case ImprovementType.DisableQuality:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Quality objQuality in _objCharacter.Qualities)
                        {
                            if (objQuality.Name == ImprovedName
                                || string.Equals(objQuality.SourceIDString, ImprovedName, StringComparison.OrdinalIgnoreCase)
                                || lstExtraImprovedName.Contains(objQuality.Name)
                                || lstExtraImprovedName.Contains(objQuality.SourceIDString))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objQuality,
                                    nameof(Quality.Suppressed));
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                                    nameof(Character.Qualities));
                            }
                        }
                    }
                    else
                    {
                        Quality objQuality = _objCharacter.Qualities.FirstOrDefault(x =>
                            x.Name == ImprovedName || string.Equals(x.SourceIDString, ImprovedName, StringComparison.OrdinalIgnoreCase));
                        if (objQuality != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objQuality,
                                nameof(Quality.Suppressed));
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                                nameof(Character.Qualities));
                        }
                    }
                }
                    break;

                case ImprovementType.FreeQuality:
                {
                    if (lstExtraImprovedName?.Count > 0)
                    {
                        foreach (Quality objQuality in _objCharacter.Qualities)
                        {
                            if (objQuality.Name == ImprovedName
                                || string.Equals(objQuality.SourceIDString, ImprovedName, StringComparison.OrdinalIgnoreCase)
                                || lstExtraImprovedName.Contains(objQuality.Name)
                                || lstExtraImprovedName.Contains(objQuality.SourceIDString))
                            {
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objQuality,
                                    nameof(Quality.ContributeToBP));
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(objQuality,
                                    nameof(Quality.ContributeToLimit));
                                yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                                    nameof(Character.Qualities));
                            }
                        }
                    }
                    else
                    {
                        Quality objQuality = _objCharacter.Qualities.FirstOrDefault(x =>
                            x.Name == ImprovedName || string.Equals(x.SourceIDString, ImprovedName, StringComparison.OrdinalIgnoreCase));
                        if (objQuality != null)
                        {
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objQuality,
                                nameof(Quality.ContributeToBP));
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(objQuality,
                                nameof(Quality.ContributeToLimit));
                            yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                                nameof(Character.Qualities));
                        }
                    }
                }
                    break;

                case ImprovementType.AllowSpriteFettering:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.AllowSpriteFettering));
                    break;
                }
                case ImprovementType.Surprise:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.Surprise));
                    break;
                }
                case ImprovementType.AstralReputation:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.AstralReputation));
                    break;
                }
                case ImprovementType.AstralReputationWild:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.WildReputation));
                    break;
                }
                case ImprovementType.CyberadeptDaemon:
                {
                    if (_objCharacter.Settings.SpecialKarmaCostBasedOnShownValue)
                        yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                            nameof(Character.CyberwareEssence));
                    break;
                }
                case ImprovementType.PenaltyFreeSustain:
                {
                    yield return new Tuple<INotifyMultiplePropertyChanged, string>(_objCharacter,
                        nameof(Character.SustainingPenalty));
                    break;
                }
            }
        }

        #region UI Methods

        public TreeNode CreateTreeNode(ContextMenuStrip cmsImprovement)
        {
            TreeNode nodImprovement = new TreeNode
            {
                Tag = this,
                Text = CustomName,
                ToolTipText = Notes.WordWrap(),
                ContextMenuStrip = cmsImprovement,
                ForeColor = PreferredColor
            };
            return nodImprovement;
        }

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return !Enabled
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }

                return !Enabled
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        #endregion UI Methods

        #endregion Methods

        public string InternalId => SourceName;
    }
}
