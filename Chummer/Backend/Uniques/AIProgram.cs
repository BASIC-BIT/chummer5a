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

using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using NLog;

namespace Chummer
{
    /// <summary>
    /// An AI Program or Advanced Program.
    /// </summary>
    [HubClassTag("SourceID", true, "Name", "Extra")]
    [DebuggerDisplay("{DisplayNameShort(GlobalSettings.DefaultLanguage)}")]
    public class AIProgram : IHasInternalId, IHasName, IHasSourceId, IHasXmlDataNode, IHasNotes, ICanRemove, IHasSource
    {
        private static readonly Lazy<Logger> s_ObjLogger = new Lazy<Logger>(LogManager.GetCurrentClassLogger);
        private static Logger Log => s_ObjLogger.Value;
        private Guid _guiID;
        private Guid _guiSourceID;
        private string _strName = string.Empty;
        private string _strRequiresProgram = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;
        private string _strExtra = string.Empty;
        private bool _blnIsAdvancedProgram;
        private bool _blnCanDelete = true;
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods

        public AIProgram(Character objCharacter)
        {
            // Create the GUID for the new Program.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// <summary>
        /// Create a Program from an XmlNode.
        /// </summary>
        /// <param name="objXmlProgramNode">XmlNode to create the object from.</param>
        /// <param name="strExtra">Value to forcefully select for any ImprovementManager prompts.</param>
        /// <param name="blnCanDelete">Can this AI program be deleted on its own (set to false for Improvement-granted programs).</param>
        public void Create(XmlNode objXmlProgramNode, string strExtra = "", bool blnCanDelete = true)
        {
            if (!objXmlProgramNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for program xmlnode", objXmlProgramNode });
                Utils.BreakIfDebug();
            }

            if (objXmlProgramNode.TryGetStringFieldQuickly("name", ref _strName))
            {
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }

            _blnCanDelete = blnCanDelete;
            objXmlProgramNode.TryGetStringFieldQuickly("require", ref _strRequiresProgram);
            objXmlProgramNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlProgramNode.TryGetStringFieldQuickly("page", ref _strPage);
            if (!objXmlProgramNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlProgramNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlProgramNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            if (GlobalSettings.InsertPdfNotesIfAvailable && string.IsNullOrEmpty(Notes))
            {
                Notes = CommonFunctions.GetBookNotes(objXmlProgramNode, Name, CurrentDisplayNameShort, Source, Page,
                    DisplayPage(GlobalSettings.Language), _objCharacter);
            }

            _strExtra = strExtra;
            string strCategory = string.Empty;
            if (objXmlProgramNode.TryGetStringFieldQuickly("category", ref strCategory))
                _blnIsAdvancedProgram = strCategory == "Advanced Programs";
        }

        private SourceString _objCachedSourceDetail;

        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == default)
                    _objCachedSourceDetail = SourceString.GetSourceString(Source,
                        DisplayPage(GlobalSettings.Language), GlobalSettings.Language, GlobalSettings.CultureInfo,
                        _objCharacter);
                return _objCachedSourceDetail;
            }
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("aiprogram");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("candelete", _blnCanDelete.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("isadvancedprogram", _blnIsAdvancedProgram.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("requiresprogram", _strRequiresProgram);
            objWriter.WriteElementString("extra", _strExtra);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("notes", _strNotes.CleanOfInvalidUnicodeChars());
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the Program from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (objNode == null)
                return;
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }

            objNode.TryGetStringFieldQuickly("name", ref _strName);
            _objCachedMyXmlNode = null;
            _objCachedMyXPathNode = null;
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                this.GetNodeXPath()?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }

            objNode.TryGetBoolFieldQuickly("candelete", ref _blnCanDelete);
            objNode.TryGetBoolFieldQuickly("isadvancedprogram", ref _blnIsAdvancedProgram);
            objNode.TryGetStringFieldQuickly("requiresprogram", ref _strRequiresProgram);
            // Legacy fix for weird, old way of storing this value
            if (_strRequiresProgram == LanguageManager.GetString("String_None")
                || _strRequiresProgram == LanguageManager.GetString("String_None", GlobalSettings.DefaultLanguage))
                _strRequiresProgram = string.Empty;
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            objNode.TryGetStringFieldQuickly("extra", ref _strExtra);
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            string sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        /// <param name="token">Cancellation token to listen to.</param>
        public async ValueTask Print(XmlWriter objWriter, string strLanguageToPrint, CancellationToken token = default)
        {
            if (objWriter == null)
                return;
            // <aiprogram>
            XmlElementWriteHelper objBaseElement = await objWriter.StartElementAsync("aiprogram", token).ConfigureAwait(false);
            try
            {
                await objWriter.WriteElementStringAsync("guid", InternalId, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("sourceid", SourceIDString, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name", await DisplayNameShortAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("fullname", await DisplayNameAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("name_english", Name, token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("requiresprogram", await DisplayRequiresProgramAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("source", await _objCharacter.LanguageBookShortAsync(Source, strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                await objWriter.WriteElementStringAsync("page", await DisplayPageAsync(strLanguageToPrint, token).ConfigureAwait(false), token).ConfigureAwait(false);
                if (GlobalSettings.PrintNotes)
                    await objWriter.WriteElementStringAsync("notes", Notes, token).ConfigureAwait(false);
            }
            finally
            {
                // </aiprogram>
                await objBaseElement.DisposeAsync().ConfigureAwait(false);
            }
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID => _guiSourceID;

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Internal identifier which will be used to identify this AI Program in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// AI Program's name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (Interlocked.Exchange(ref _strName, value) == value)
                    return;
                _objCachedMyXmlNode = null;
                _objCachedMyXPathNode = null;
            }
        }

        /// <summary>
        /// AI Program's extra info.
        /// </summary>
        public string Extra
        {
            get => _strExtra;
            set => _strExtra = _objCharacter.ReverseTranslateExtra(value);
        }

        public string DisplayName(string strLanguage)
        {
            return DisplayNameShort(strLanguage);
        }

        public ValueTask<string> DisplayNameAsync(string strLanguage, CancellationToken token = default)
        {
            return DisplayNameShortAsync(strLanguage, token);
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            string strReturn = Name;
            // Get the translated name if applicable.
            if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                strReturn = this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? Name;

            if (!string.IsNullOrEmpty(Extra))
            {
                string strExtra = Extra;
                if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    strExtra = _objCharacter.TranslateExtra(Extra, strLanguage);
                strReturn += LanguageManager.GetString("String_Space", strLanguage) + '(' + strExtra + ')';
            }
            return strReturn;
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public async ValueTask<string> DisplayNameShortAsync(string strLanguage, CancellationToken token = default)
        {
            string strReturn = Name;
            // Get the translated name if applicable.
            if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
                strReturn = objNode != null ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token).ConfigureAwait(false))?.Value ?? Name : Name;
            }

            if (!string.IsNullOrEmpty(Extra))
            {
                string strExtra = Extra;
                if (!strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                    strExtra = await _objCharacter.TranslateExtraAsync(Extra, strLanguage, token: token).ConfigureAwait(false);
                strReturn += await LanguageManager.GetStringAsync("String_Space", strLanguage, token: token).ConfigureAwait(false) + '(' + strExtra + ')';
            }
            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplayNameAsync(CancellationToken token = default) => DisplayNameAsync(GlobalSettings.Language, token);

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string CurrentDisplayNameShort => DisplayNameShort(GlobalSettings.Language);

        public ValueTask<string> GetCurrentDisplayNameShortAsync(CancellationToken token = default) => DisplayNameShortAsync(GlobalSettings.Language, token);

        /// <summary>
        /// AI Advanced Program's requirement program.
        /// </summary>
        public string RequiresProgram
        {
            get => _strRequiresProgram;
            set => _strRequiresProgram = value;
        }

        /// <summary>
        /// AI Advanced Program's requirement program.
        /// </summary>
        public string DisplayRequiresProgram(string strLanguage)
        {
            if (string.IsNullOrEmpty(RequiresProgram))
                return LanguageManager.GetString("String_None", strLanguage);
            if (strLanguage == GlobalSettings.Language)
                return RequiresProgram;

            return _objCharacter.LoadDataXPath("programs.xml", strLanguage)
                                .TryGetNodeByNameOrId("/chummer/programs/program", RequiresProgram)
                                ?.SelectSingleNodeAndCacheExpression("translate")?.Value ?? RequiresProgram;
        }

        /// <summary>
        /// AI Advanced Program's requirement program.
        /// </summary>
        public async ValueTask<string> DisplayRequiresProgramAsync(string strLanguage, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(RequiresProgram))
                return await LanguageManager.GetStringAsync("String_None", strLanguage, token: token).ConfigureAwait(false);
            if (strLanguage == GlobalSettings.Language)
                return RequiresProgram;

            XPathNavigator xmlRequiresProgramNode
                = (await _objCharacter.LoadDataXPathAsync("programs.xml", strLanguage, token: token)
                                      .ConfigureAwait(false))
                .TryGetNodeByNameOrId("/chummer/programs/program", RequiresProgram);
            return xmlRequiresProgramNode != null
                ? (await xmlRequiresProgramNode.SelectSingleNodeAndCacheExpressionAsync("translate", token: token)
                                               .ConfigureAwait(false))?.Value ?? RequiresProgram
                : RequiresProgram;
        }

        /// <summary>
        /// If the AI Advanced Program is added from a quality.
        /// </summary>
        public bool CanDelete
        {
            get => _blnCanDelete;
            set => _blnCanDelete = value;
        }

        /// <summary>
        /// AI Program's Source.
        /// </summary>
        public string Source
        {
            get => _strSource;
            set => _strSource = value;
        }

        /// <summary>
        /// Sourcebook Page Number.
        /// </summary>
        public string Page
        {
            get => _strPage;
            set => _strPage = value;
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <returns></returns>
        public string DisplayPage(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            string s = this.GetNodeXPath(strLanguage)?.SelectSingleNodeAndCacheExpression("altpage")?.Value ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns></returns>
        public async ValueTask<string> DisplayPageAsync(string strLanguage, CancellationToken token = default)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            XPathNavigator objNode = await this.GetNodeXPathAsync(strLanguage, token: token).ConfigureAwait(false);
            string s = objNode != null
                ? (await objNode.SelectSingleNodeAndCacheExpressionAsync("altpage", token: token).ConfigureAwait(false))?.Value ?? Page
                : Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Notes.
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
        /// If the AI Program is an Advanced Program.
        /// </summary>
        public bool IsAdvancedProgram => _blnIsAdvancedProgram;

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public async Task<XmlNode> GetNodeCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XmlNode objReturn = _objCachedMyXmlNode;
            if (objReturn != null && strLanguage == _strCachedXmlNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XmlNode objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadData("programs.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataAsync("programs.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/programs/program", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/programs/program", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXmlNode = objReturn;
            _strCachedXmlNodeLanguage = strLanguage;
            return objReturn;
        }

        private XPathNavigator _objCachedMyXPathNode;
        private string _strCachedXPathNodeLanguage = string.Empty;

        public async Task<XPathNavigator> GetNodeXPathCoreAsync(bool blnSync, string strLanguage, CancellationToken token = default)
        {
            XPathNavigator objReturn = _objCachedMyXPathNode;
            if (objReturn != null && strLanguage == _strCachedXPathNodeLanguage
                                  && !GlobalSettings.LiveCustomData)
                return objReturn;
            XPathNavigator objDoc = blnSync
                // ReSharper disable once MethodHasAsyncOverload
                ? _objCharacter.LoadDataXPath("programs.xml", strLanguage, token: token)
                : await _objCharacter.LoadDataXPathAsync("programs.xml", strLanguage, token: token).ConfigureAwait(false);
            if (SourceID != Guid.Empty)
                objReturn = objDoc.TryGetNodeById("/chummer/programs/program", SourceID);
            if (objReturn == null)
            {
                objReturn = objDoc.TryGetNodeByNameOrId("/chummer/programs/program", Name);
                objReturn?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            _objCachedMyXPathNode = objReturn;
            _strCachedXPathNodeLanguage = strLanguage;
            return objReturn;
        }

        #endregion Properties

        #region UI Methods

        public TreeNode CreateTreeNode(ContextMenuStrip cmsAIProgram)
        {
            if (!CanDelete && !string.IsNullOrEmpty(Source) && !_objCharacter.Settings.BookEnabled(Source))
                return null;

            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = CurrentDisplayNameShort,
                Tag = this,
                ContextMenuStrip = cmsAIProgram,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };
            return objNode;
        }

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return !CanDelete
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }
                return !CanDelete
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        #endregion UI Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (!CanDelete)
                return false;
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteAIProgram")))
                return false;

            ImprovementManager.RemoveImprovements(_objCharacter, Improvement.ImprovementSource.AIProgram,
                InternalId);

            return _objCharacter.AIPrograms.Remove(this);
        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            SourceDetail.SetControl(sourceControl);
        }

        public Task SetSourceDetailAsync(Control sourceControl, CancellationToken token = default)
        {
            if (_objCachedSourceDetail.Language != GlobalSettings.Language)
                _objCachedSourceDetail = default;
            return SourceDetail.SetControlAsync(sourceControl, token);
        }
    }
}
