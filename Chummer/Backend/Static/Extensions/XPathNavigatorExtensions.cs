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
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;

namespace Chummer
{
    public static class XPathNavigatorExtensions
    {
        public delegate bool TryParseFunction<T>(string input, out T result);

        /// <summary>
        /// This method is syntactic sugar for attempting to read a data field
        /// from an XmlNode. This version sets the output variable to its
        /// default value in case of a failed read and can be used for
        /// initializing variables. It can work on any type, but it requires
        /// a tryParse style function that is fed the nodes InnerText
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="node"></param>
        /// <param name="field"></param>
        /// <param name="parser"></param>
        /// <param name="read"></param>
        /// <param name="onError"></param>
        /// <returns></returns>
        public static bool TryGetField<T>(this XPathNavigator node, string field, TryParseFunction<T> parser, out T read, T onError = default)
        {
            if (parser != null)
            {
                XPathNavigator objField = node?.SelectSingleNode(field);
                if (objField != null)
                {
                    return parser(objField.Value, out read);
                }
            }

            read = onError;
            return false;
        }

        /// <summary>
        /// Processes a single operation node with children that are either nodes to check whether the parent has a node that fulfills a condition, or they are nodes that are parents to further operation nodes
        /// </summary>
        /// <param name="blnIsOrNode">Whether this is an OR node (true) or an AND node (false). Default is AND (false).</param>
        /// <param name="xmlOperationNode">The node containing the filter operation or a list of filter operations. Every element here is checked against corresponding elements in the parent node, using an operation specified in the element's attributes.</param>
        /// <param name="xmlParentNode">The parent node against which the filter operations are checked.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>True if the parent node passes the conditions set in the operation node/nodelist, false otherwise.</returns>
        public static bool ProcessFilterOperationNode(this XPathNavigator xmlParentNode,
                                                      XPathNavigator xmlOperationNode, bool blnIsOrNode, CancellationToken token = default)
        {
            return Utils.SafelyRunSynchronously(() => ProcessFilterOperationNodeCoreAsync(true, xmlParentNode, xmlOperationNode, blnIsOrNode, token), token);
        }

        /// <summary>
        /// Processes a single operation node with children that are either nodes to check whether the parent has a node that fulfills a condition, or they are nodes that are parents to further operation nodes
        /// </summary>
        /// <param name="blnIsOrNode">Whether this is an OR node (true) or an AND node (false). Default is AND (false).</param>
        /// <param name="xmlOperationNode">The node containing the filter operation or a list of filter operations. Every element here is checked against corresponding elements in the parent node, using an operation specified in the element's attributes.</param>
        /// <param name="xmlParentNode">The parent node against which the filter operations are checked.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>True if the parent node passes the conditions set in the operation node/nodelist, false otherwise.</returns>
        public static Task<bool> ProcessFilterOperationNodeAsync(this XPathNavigator xmlParentNode,
                                                                 XPathNavigator xmlOperationNode, bool blnIsOrNode, CancellationToken token = default)
        {
            return ProcessFilterOperationNodeCoreAsync(false, xmlParentNode, xmlOperationNode, blnIsOrNode, token);
        }

        /// <summary>
        /// Processes a single operation node with children that are either nodes to check whether the parent has a node that fulfills a condition, or they are nodes that are parents to further operation nodes
        /// Uses flag hack method design outlined here to avoid locking:
        /// https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development
        /// </summary>
        /// <param name="blnSync">Flag for whether method should always use synchronous code or not.</param>
        /// <param name="blnIsOrNode">Whether this is an OR node (true) or an AND node (false). Default is AND (false).</param>
        /// <param name="xmlOperationNode">The node containing the filter operation or a list of filter operations. Every element here is checked against corresponding elements in the parent node, using an operation specified in the element's attributes.</param>
        /// <param name="xmlParentNode">The parent node against which the filter operations are checked.</param>
        /// <param name="token">Cancellation token to listen to.</param>
        /// <returns>True if the parent node passes the conditions set in the operation node/nodelist, false otherwise.</returns>
        private static async Task<bool> ProcessFilterOperationNodeCoreAsync(bool blnSync, XPathNavigator xmlParentNode, XPathNavigator xmlOperationNode, bool blnIsOrNode, CancellationToken token = default)
        {
            if (xmlOperationNode == null)
                return false;

            foreach (XPathNavigator xmlOperationChildNode in xmlOperationNode.SelectChildren(XPathNodeType.Element))
            {
                bool blnInvert
                    = (blnSync
                        // ReSharper disable once MethodHasAsyncOverload
                        ? xmlOperationChildNode.SelectSingleNodeAndCacheExpression("@NOT", token)
                        : await xmlOperationChildNode.SelectSingleNodeAndCacheExpressionAsync("@NOT", token: token).ConfigureAwait(false)) != null;

                bool blnOperationChildNodeResult = blnInvert;
                string strNodeName = xmlOperationChildNode.Name;
                switch (strNodeName)
                {
                    case "OR":
                        blnOperationChildNodeResult =
                            (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? ProcessFilterOperationNode(xmlParentNode, xmlOperationChildNode, true, token)
                                : await ProcessFilterOperationNodeAsync(xmlParentNode, xmlOperationChildNode, true, token).ConfigureAwait(false))
                            != blnInvert;
                        break;

                    case "NOR":
                        blnOperationChildNodeResult =
                            (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? ProcessFilterOperationNode(xmlParentNode, xmlOperationChildNode, true, token)
                                : await ProcessFilterOperationNodeAsync(xmlParentNode, xmlOperationChildNode, true, token).ConfigureAwait(false))
                            == blnInvert;
                        break;

                    case "AND":
                        blnOperationChildNodeResult =
                            (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? ProcessFilterOperationNode(xmlParentNode, xmlOperationChildNode, false, token)
                                : await ProcessFilterOperationNodeAsync(xmlParentNode, xmlOperationChildNode, false, token).ConfigureAwait(false))
                            != blnInvert;
                        break;

                    case "NAND":
                        blnOperationChildNodeResult =
                            (blnSync
                                // ReSharper disable once MethodHasAsyncOverload
                                ? ProcessFilterOperationNode(xmlParentNode, xmlOperationChildNode, false, token)
                                : await ProcessFilterOperationNodeAsync(xmlParentNode, xmlOperationChildNode, false, token).ConfigureAwait(false))
                            == blnInvert;
                        break;

                    case "NONE":
                        blnOperationChildNodeResult = (xmlParentNode == null) != blnInvert;
                        break;

                    default:
                        {
                            if (xmlParentNode != null)
                            {
                                XPathNavigator objOperationAttribute = blnSync
                                    // ReSharper disable once MethodHasAsyncOverload
                                    ? xmlOperationChildNode.SelectSingleNodeAndCacheExpression("@operation", token)
                                    : await xmlOperationChildNode.SelectSingleNodeAndCacheExpressionAsync("@operation", token: token).ConfigureAwait(false);
                                string strOperationType = objOperationAttribute?.Value ?? "==";
                                XPathNodeIterator objXmlTargetNodeList = xmlParentNode.Select(strNodeName);
                                // If we're just checking for existence of a node, no need for more processing
                                if (strOperationType == "exists")
                                {
                                    blnOperationChildNodeResult = (objXmlTargetNodeList.Count > 0) != blnInvert;
                                }
                                else
                                {
                                    bool blnOperationChildNodeAttributeOr = (blnSync
                                        ? xmlOperationChildNode
                                            // ReSharper disable once MethodHasAsyncOverload
                                            .SelectSingleNodeAndCacheExpression("@OR", token)
                                        : await xmlOperationChildNode
                                            .SelectSingleNodeAndCacheExpressionAsync("@OR", token: token).ConfigureAwait(false)) != null;
                                    // default is "any", replace with switch() if more check modes are necessary
                                    XPathNavigator objCheckTypeAttribute = blnSync
                                        // ReSharper disable once MethodHasAsyncOverload
                                        ? xmlOperationChildNode.SelectSingleNodeAndCacheExpression("@checktype", token)
                                        : await xmlOperationChildNode.SelectSingleNodeAndCacheExpressionAsync("@checktype", token: token).ConfigureAwait(false);
                                    bool blnCheckAll = objCheckTypeAttribute?.Value == "all";
                                    blnOperationChildNodeResult = blnCheckAll;
                                    string strOperationChildNodeText = xmlOperationChildNode.Value;
                                    bool blnOperationChildNodeEmpty = string.IsNullOrWhiteSpace(strOperationChildNodeText);

                                    foreach (XPathNavigator xmlTargetNode in objXmlTargetNodeList)
                                    {
                                        bool boolSubNodeResult = blnInvert;
                                        if (xmlTargetNode.SelectChildren(XPathNodeType.Element).Count > 0)
                                        {
                                            if (xmlOperationChildNode.SelectChildren(XPathNodeType.Element).Count > 0)
                                                boolSubNodeResult = (blnSync
                                                                        // ReSharper disable once MethodHasAsyncOverload
                                                                        ? ProcessFilterOperationNode(xmlTargetNode,
                                                                            xmlOperationChildNode,
                                                                            blnOperationChildNodeAttributeOr, token)
                                                                        : await ProcessFilterOperationNodeAsync(
                                                                            xmlTargetNode,
                                                                            xmlOperationChildNode,
                                                                            blnOperationChildNodeAttributeOr, token).ConfigureAwait(false))
                                                                    != blnInvert;
                                        }
                                        else
                                        {
                                            string strTargetNodeText = xmlTargetNode.Value;
                                            bool blnTargetNodeEmpty = string.IsNullOrWhiteSpace(strTargetNodeText);
                                            if (blnTargetNodeEmpty || blnOperationChildNodeEmpty)
                                            {
                                                if (blnTargetNodeEmpty == blnOperationChildNodeEmpty
                                                    && (strOperationType == "=="
                                                        || strOperationType == "equals"))
                                                {
                                                    boolSubNodeResult = !blnInvert;
                                                }
                                                else
                                                {
                                                    boolSubNodeResult = blnInvert;
                                                }
                                            }
                                            // Note when adding more operation cases: XML does not like the "<" symbol as part of an attribute value
                                            else
                                                switch (strOperationType)
                                                {
                                                    case "doesnotequal":
                                                    case "notequals":
                                                    case "!=":
                                                        blnInvert = !blnInvert;
                                                        goto default;
                                                    case "lessthan":
                                                        blnInvert = !blnInvert;
                                                        goto case ">=";
                                                    case "lessthanequals":
                                                        blnInvert = !blnInvert;
                                                        goto case ">";

                                                    case "like":
                                                    case "contains":
                                                        {
                                                            boolSubNodeResult =
                                                                strTargetNodeText.Contains(strOperationChildNodeText, StringComparison.OrdinalIgnoreCase)
                                                                != blnInvert;
                                                            break;
                                                        }
                                                    case "greaterthan":
                                                    case ">":
                                                        {
                                                            boolSubNodeResult =
                                                                (int.TryParse(strTargetNodeText, out int intTargetNodeValue)
                                                                 && int.TryParse(strOperationChildNodeText, out int intChildNodeValue)
                                                                 && intTargetNodeValue > intChildNodeValue)
                                                                != blnInvert;
                                                            break;
                                                        }
                                                    case "greaterthanequals":
                                                    case ">=":
                                                        {
                                                            boolSubNodeResult =
                                                                (int.TryParse(strTargetNodeText, out int intTargetNodeValue)
                                                                 && int.TryParse(strOperationChildNodeText, out int intChildNodeValue)
                                                                 && intTargetNodeValue >= intChildNodeValue)
                                                                != blnInvert;
                                                            break;
                                                        }
                                                    default:
                                                        boolSubNodeResult =
                                                            (strTargetNodeText.Trim() == strOperationChildNodeText.Trim())
                                                            != blnInvert;
                                                        break;
                                                }
                                        }

                                        if (blnCheckAll)
                                        {
                                            if (!boolSubNodeResult)
                                            {
                                                blnOperationChildNodeResult = false;
                                                break;
                                            }
                                        }
                                        // default is "any", replace above with a switch() should more than two check types be required
                                        else if (boolSubNodeResult)
                                        {
                                            blnOperationChildNodeResult = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            break;
                        }
                }

                switch (blnIsOrNode)
                {
                    case true when blnOperationChildNodeResult:
                        return true;

                    case false when !blnOperationChildNodeResult:
                        return false;
                }
            }

            return !blnIsOrNode;
        }

        /// <summary>
        /// Like TryGetField for strings, only with as little overhead as possible.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetStringFieldQuickly(this XPathNavigator node, string field, ref string read)
        {
            if (node == null)
                return false;
            XPathNavigator objField = Utils.CachedXPathExpressions.TryGetValue(field, out XPathExpression objCachedExpression)
                ? node.SelectSingleNode(objCachedExpression)
                : node.SelectSingleNode(field);
            if (objField == null && !field.StartsWith('@'))
            {
                field = '@' + field;
                objField = Utils.CachedXPathExpressions.TryGetValue(field, out objCachedExpression)
                    ? node.SelectSingleNode(objCachedExpression)
                    : node.SelectSingleNode(field);
            }
            if (objField != null)
            {
                read = objField.Value;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Like TryGetField for strings, only with as little overhead as possible and with an extra line ending normalization thrown in.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetMultiLineStringFieldQuickly(this XPathNavigator node, string field, ref string read)
        {
            string strReturn = string.Empty;
            if (node.TryGetStringFieldQuickly(field, ref strReturn))
            {
                read = strReturn.NormalizeLineEndings();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Like TryGetField for ints, but taking advantage of int.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetInt32FieldQuickly(this XPathNavigator node, string field, ref int read, IFormatProvider objCulture = null)
        {
            if (node == null)
                return false;
            XPathNavigator objField = Utils.CachedXPathExpressions.TryGetValue(field, out XPathExpression objCachedExpression)
                ? node.SelectSingleNode(objCachedExpression)
                : node.SelectSingleNode(field);
            if (objField != null)
            {
                if (objCulture == null)
                    objCulture = GlobalSettings.InvariantCultureInfo;
                if (int.TryParse(objField.Value, NumberStyles.Any, objCulture, out int intTmp))
                {
                    read = intTmp;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Like TryGetField for bools, but taking advantage of bool.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetBoolFieldQuickly(this XPathNavigator node, string field, ref bool read)
        {
            if (node == null)
                return false;
            XPathNavigator objField = Utils.CachedXPathExpressions.TryGetValue(field, out XPathExpression objCachedExpression)
                ? node.SelectSingleNode(objCachedExpression)
                : node.SelectSingleNode(field);
            if (objField != null && bool.TryParse(objField.Value, out bool blnTmp))
            {
                read = blnTmp;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Like TryGetField for decimals, but taking advantage of decimal.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetDecFieldQuickly(this XPathNavigator node, string field, ref decimal read, IFormatProvider objCulture = null)
        {
            if (node == null)
                return false;
            XPathNavigator objField = Utils.CachedXPathExpressions.TryGetValue(field, out XPathExpression objCachedExpression)
                ? node.SelectSingleNode(objCachedExpression)
                : node.SelectSingleNode(field);
            if (objField != null)
            {
                if (objCulture == null)
                    objCulture = GlobalSettings.InvariantCultureInfo;
                if (decimal.TryParse(objField.Value, NumberStyles.Any, objCulture, out decimal decTmp))
                {
                    read = decTmp;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Like TryGetField for doubles, but taking advantage of double.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetDoubleFieldQuickly(this XPathNavigator node, string field, ref double read, IFormatProvider objCulture = null)
        {
            if (node == null)
                return false;
            XPathNavigator objField = Utils.CachedXPathExpressions.TryGetValue(field, out XPathExpression objCachedExpression)
                ? node.SelectSingleNode(objCachedExpression)
                : node.SelectSingleNode(field);
            if (objField != null)
            {
                if (objCulture == null)
                    objCulture = GlobalSettings.InvariantCultureInfo;
                if (double.TryParse(objField.Value, NumberStyles.Any, objCulture, out double dblTmp))
                {
                    read = dblTmp;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Like TryGetField for float, but taking advantage of float.TryParse... boo, no TryParse interface! :(
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetFloatFieldQuickly(this XPathNavigator node, string field, ref float read, IFormatProvider objCulture = null)
        {
            if (node == null)
                return false;
            XPathNavigator objField = Utils.CachedXPathExpressions.TryGetValue(field, out XPathExpression objCachedExpression)
                ? node.SelectSingleNode(objCachedExpression)
                : node.SelectSingleNode(field);
            if (objField != null)
            {
                if (objCulture == null)
                    objCulture = GlobalSettings.InvariantCultureInfo;
                if (float.TryParse(objField.Value, NumberStyles.Any, objCulture, out float fltTmp))
                {
                    read = fltTmp;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Like TryGetField for guids, but taking advantage of guid.TryParse. Allows for returning false if the guid is Empty.
        /// </summary>
        /// <param name="node">XPathNavigator node of the object.</param>
        /// <param name="field">Field name of the InnerXML element we're looking for.</param>
        /// <param name="read">Guid that will be returned.</param>
        /// <param name="falseIfEmpty">Defaults to true. If false, will return an empty Guid if the returned Guid field is empty.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool TryGetGuidFieldQuickly(this XPathNavigator node, string field, ref Guid read, bool falseIfEmpty = true)
        {
            if (node == null)
                return false;
            XPathNavigator objField = Utils.CachedXPathExpressions.TryGetValue(field, out XPathExpression objCachedExpression)
                ? node.SelectSingleNode(objCachedExpression)
                : node.SelectSingleNode(field);
            if (objField == null)
                return false;
            if (!Guid.TryParse(objField.Value, out Guid guidTmp))
                return false;
            if (guidTmp == Guid.Empty && falseIfEmpty)
            {
                return false;
            }
            read = guidTmp;
            return true;
        }

        /// <summary>
        /// Determine whether or not an XPathNavigator with the specified name exists within an XPathNavigator.
        /// </summary>
        /// <param name="xmlNode">XPathNavigator to examine.</param>
        /// <param name="strName">Name of the XPathNavigator to look for.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NodeExists(this XPathNavigator xmlNode, string strName)
        {
            if (string.IsNullOrEmpty(strName) || xmlNode == null)
                return false;
            XPathNavigator objField = Utils.CachedXPathExpressions.TryGetValue(strName, out XPathExpression objCachedExpression)
                ? xmlNode.SelectSingleNode(objCachedExpression)
                : xmlNode.SelectSingleNode(strName);
            return objField != null;
        }

        /// <summary>
        /// Query the XPathNavigator for a given node with an id or name element. Includes ToUpperInvariant processing to handle uppercase ids.
        /// </summary>
        /// <param name="node">XPathNavigator to examine.</param>
        /// <param name="strPath">Name of the XPathNavigator to look for.</param>
        /// <param name="strId">Element to search for. If it parses as a guid or f it fails to parse as a guid AND blnIdIsGuid is set, it will still search for id, otherwise it will search for a node with a name element that matches.</param>
        /// <param name="strExtraXPath">'Extra' value to append to the search.</param>
        /// <param name="blnIdIsGuid">Whether to evaluate the ID as a GUID or a string. Use false to pass strId as a string.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XPathNavigator TryGetNodeByNameOrId(this XPathNavigator node, string strPath, string strId, string strExtraXPath = "", bool blnIdIsGuid = true)
        {
            if (node == null || string.IsNullOrEmpty(strPath) || string.IsNullOrEmpty(strId))
                return null;

            if (Guid.TryParse(strId, out Guid guidId))
            {
                XPathNavigator objReturn = node.TryGetNodeById(strPath, guidId, strExtraXPath);
                if (objReturn != null)
                    return objReturn;
            }
            // This is mostly for improvements.xml, which uses the improvement id (such as addecho) as the id rather than a guid.
            if (!blnIdIsGuid)
            {
                return node.SelectSingleNode(strPath + "[id = " + strId.CleanXPath()
                                             + (string.IsNullOrEmpty(strExtraXPath)
                                                 ? "]"
                                                 : " and (" + strExtraXPath + ") ]"));
            }

            return node.SelectSingleNode(strPath + "[name = " + strId.CleanXPath()
                                         + (string.IsNullOrEmpty(strExtraXPath)
                                             ? "]"
                                             : " and (" + strExtraXPath + ") ]"));
        }

        /// <summary>
        /// Query the XPathNavigator for a given node with an id. Includes ToUpperInvariant processing to handle uppercase ids.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XPathNavigator TryGetNodeById(this XPathNavigator node, string strPath, Guid guidId, string strExtraXPath = "")
        {
            if (node == null || string.IsNullOrEmpty(strPath))
                return null;
            string strId = guidId.ToString("D", GlobalSettings.InvariantCultureInfo);
            return node.SelectSingleNode(strPath + "[id = " + strId.CleanXPath()
                                         + (string.IsNullOrEmpty(strExtraXPath)
                                             ? "]"
                                             : " and (" + strExtraXPath + ")]"))
                   // Split into two separate queries because the case-insensitive search here can be expensive if we're doing it a lot
                   ?? node.SelectSingleNode(strPath + "[translate(id, 'abcdef', 'ABCDEF') = "
                                                    + strId.ToUpperInvariant().CleanXPath()
                                                    + (string.IsNullOrEmpty(strExtraXPath)
                                                        ? "]"
                                                        : " and (" + strExtraXPath + ")]"));
        }

        /// <summary>
        /// Create a new XmlNode in an XmlDocument based on the contents of an XPathNavigator
        /// </summary>
        /// <param name="xmlNode">XPathNavigator to examine.</param>
        /// <param name="xmlParentDocument">Document to house the XmlNode</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XmlNode ToXmlNode(this XPathNavigator xmlNode, XmlDocument xmlParentDocument)
        {
            if (xmlNode == null || xmlParentDocument == null)
                return null;
            XmlNodeType eNodeType;
            switch (xmlNode.NodeType)
            {
                case XPathNodeType.Root:
                    eNodeType = XmlNodeType.Document;
                    break;

                case XPathNodeType.Element:
                    eNodeType = XmlNodeType.Element;
                    break;

                case XPathNodeType.Attribute:
                    eNodeType = XmlNodeType.Attribute;
                    break;

                case XPathNodeType.Namespace:
                    eNodeType = XmlNodeType.XmlDeclaration;
                    break;

                case XPathNodeType.Text:
                    eNodeType = XmlNodeType.Text;
                    break;

                case XPathNodeType.SignificantWhitespace:
                    eNodeType = XmlNodeType.SignificantWhitespace;
                    break;

                case XPathNodeType.Whitespace:
                    eNodeType = XmlNodeType.Whitespace;
                    break;

                case XPathNodeType.ProcessingInstruction:
                    eNodeType = XmlNodeType.ProcessingInstruction;
                    break;

                case XPathNodeType.Comment:
                    eNodeType = XmlNodeType.Comment;
                    break;

                case XPathNodeType.All:
                    eNodeType = XmlNodeType.None;
                    Utils.BreakIfDebug();
                    break;

                default:
                    throw new InvalidOperationException(nameof(xmlNode.NodeType));
            }
            XmlNode xmlReturn = xmlParentDocument.CreateNode(eNodeType, xmlNode.Prefix, xmlNode.Name, xmlNode.NamespaceURI);
            xmlReturn.InnerXml = xmlNode.InnerXml;
            return xmlReturn;
        }

        /// <summary>
        /// Selects a single node using the specified XPath expression, but also caches that expression in case the same expression is used over and over.
        /// Effectively a version of SelectSingleNode(string xpath) that is slower on the first run (and consumes some memory), but faster on subsequent runs.
        /// Only use this if there's a particular XPath expression that keeps being used over and over.
        /// </summary>
        public static XPathNavigator SelectSingleNodeAndCacheExpression(this XPathNavigator xmlNode, string xpath, CancellationToken token = default)
        {
            XPathExpression objExpression = Utils.CachedXPathExpressions.AddOrGet(xpath, x => XPathExpression.Compile(xpath), token);
            return xmlNode.SelectSingleNode(objExpression);
        }

        /// <summary>
        /// Selects a single node using the specified XPath expression, but also caches that expression in case the same expression is used over and over.
        /// Effectively a version of SelectSingleNode(string xpath) that is slower on the first run (and consumes some memory), but faster on subsequent runs.
        /// Only use this if there's a particular XPath expression that keeps being used over and over.
        /// </summary>
        public static async Task<XPathNavigator> SelectSingleNodeAndCacheExpressionAsync(this XPathNavigator xmlNode, string xpath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XPathExpression objExpression = await Utils.CachedXPathExpressions.AddOrGetAsync(xpath, x => XPathExpression.Compile(xpath), token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            return xmlNode.SelectSingleNode(objExpression);
        }

        /// <summary>
        /// Selects a single node using the specified XPath expression, but also caches that expression in case the same expression is used over and over.
        /// Effectively a version of SelectSingleNode(string xpath) that is slower on the first run (and consumes some memory), but faster on subsequent runs.
        /// Only use this if there's a particular XPath expression that keeps being used over and over.
        /// </summary>
        public static async Task<XPathNavigator> SelectSingleNodeAndCacheExpressionAsync(this Task<XPathNavigator> tskNode, string xpath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XPathExpression objExpression = await Utils.CachedXPathExpressions.AddOrGetAsync(xpath, x => XPathExpression.Compile(xpath), token).ConfigureAwait(false);
            XPathNavigator xmlNode = await tskNode.ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            return xmlNode.SelectSingleNode(objExpression);
        }

        /// <summary>
        /// Selects a node set using the specified XPath expression, but also caches that expression in case the same expression is used over and over.
        /// Effectively a version of Select(string xpath) that is slower on the first run (and consumes some memory), but faster on subsequent runs.
        /// Only use this if there's a particular XPath expression that keeps being used over and over.
        /// </summary>
        public static XPathNodeIterator SelectAndCacheExpression(this XPathNavigator xmlNode, string xpath, CancellationToken token = default)
        {
            XPathExpression objExpression = Utils.CachedXPathExpressions.AddOrGet(xpath, x => XPathExpression.Compile(xpath), token);
            return xmlNode.Select(objExpression);
        }

        /// <summary>
        /// Selects a node set using the specified XPath expression, but also caches that expression in case the same expression is used over and over.
        /// Effectively a version of Select(string xpath) that is slower on the first run (and consumes some memory), but faster on subsequent runs.
        /// Only use this if there's a particular XPath expression that keeps being used over and over.
        /// </summary>
        public static async Task<XPathNodeIterator> SelectAndCacheExpressionAsync(this XPathNavigator xmlNode, string xpath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XPathExpression objExpression = await Utils.CachedXPathExpressions.AddOrGetAsync(xpath, x => XPathExpression.Compile(xpath), token).ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            return xmlNode.Select(objExpression);
        }

        /// <summary>
        /// Selects a node set using the specified XPath expression, but also caches that expression in case the same expression is used over and over.
        /// Effectively a version of Select(string xpath) that is slower on the first run (and consumes some memory), but faster on subsequent runs.
        /// Only use this if there's a particular XPath expression that keeps being used over and over.
        /// </summary>
        public static async Task<XPathNodeIterator> SelectAndCacheExpressionAsync(this Task<XPathNavigator> tskNode, string xpath, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            XPathExpression objExpression = await Utils.CachedXPathExpressions.AddOrGetAsync(xpath, x => XPathExpression.Compile(xpath), token).ConfigureAwait(false);
            XPathNavigator xmlNode = await tskNode.ConfigureAwait(false);
            token.ThrowIfCancellationRequested();
            return xmlNode.Select(objExpression);
        }
    }
}
