using CoreDll.Extensions;
using CoreDll.Extensions.Conversions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using System.Xml;


namespace CoreDll.Utils
{
    public class Xml
    {
        public bool IgnoreWhitespacesWhenSaveOrLoad { get; set; }

        public XmlDocument Document { get; set; }
        public string SetDefaultNode { get; set; }

        public Xml()
        {
            Document = new XmlDocument();
            Document.PreserveWhitespace = false;
            Document.CreateProcessingInstruction("xml", "version=\"1.0\" encoding=\"UTF-8\"");
            //XmlNode node = null;
            //this.Document.AppendChild(node);

            //this.Document = NfeCsDll.OldVB.Funcoes.Criar_XML_Doc();            
        }

        public Xml(string xml, bool ignoreWhitespacesWhenSaveOrLoad = false)
        {
            IgnoreWhitespacesWhenSaveOrLoad = ignoreWhitespacesWhenSaveOrLoad;

            string xmlFixed = FixProcessingInstruction(xml);

            if (IgnoreWhitespacesWhenSaveOrLoad)
            {
                xmlFixed = IgnoreWhitespaces(xmlFixed);
            }

            Document = new XmlDocument();
            Document.PreserveWhitespace = false;
            Document.LoadXml(xmlFixed);
        }

        public Xml(XmlDocument xml, bool ignoreWhitespacesWhenSaveOrLoad = false)
            : this(xml.OuterXml, ignoreWhitespacesWhenSaveOrLoad)
        { }

        public Xml(XmlElement xml, bool ignoreWhitespacesWhenSaveOrLoad = false)
            : this(xml.OuterXml, ignoreWhitespacesWhenSaveOrLoad)
        { }

        public Xml(XmlNode xml, bool ignoreWhitespacesWhenSaveOrLoad = false)
            : this(xml.OuterXml, ignoreWhitespacesWhenSaveOrLoad)
        { }

        private string FixProcessingInstruction(string xml)
        {
            Match match = Regex.Match(xml.Trim(), @"<\?xml[\w\W]+?\?>", RegexOptions.IgnoreCase);
            string prolog = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>";

            if (match != null && match.Index <= 5)
            {
                return xml;
            }
            else
            {
                return prolog + xml;
            }
        }

        public Xml this[string tagName]
        {
            get
            {
                Xml found = null;

                foreach (XmlNode node in Document.ChildNodes)
                {
                    if (node.Name == tagName)
                    {
                        found = new Xml(node);
                        break;
                    }
                }

                return found;
            }
            set
            {
                bool nodeFound = false;

                for (int idx = 0; idx < Document.ChildNodes.Count; idx++)
                {
                    if (Document.ChildNodes[idx].Name == tagName)
                    {
                        Document.InsertAfter(value.ToXmlNode(), Document.ChildNodes[idx]);
                        Document.RemoveChild(Document.ChildNodes[idx]);
                        nodeFound = true;
                        break;
                    }
                }

                if (!nodeFound)
                {
                    Document.AppendChild(value.ToXmlNode());

                }
            }
        }

        public bool ContainsTag(string name)
        {
            foreach (XmlNode node in Document.ChildNodes)
            {
                if (node.Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        public bool ContainsAttribute(string name)
        {
            foreach (XmlAttribute att in Document.LastChild.Attributes)
            {
                if (att.Name == name)
                {
                    return true;
                }
            }

            return false;
        }

        public string GetAttributeValue(string name)
        {
            foreach (XmlAttribute att in Document.LastChild.Attributes)
            {
                if (att.Name == name)
                {
                    return att.Value;
                }
            }

            return null;
        }

        public string GetTagValue(string name)
        {
            foreach (XmlNode node in Document.LastChild.ChildNodes)
            {
                if (node.Name == name)
                {
                    return node.Value;
                }
            }

            return null;
        }

        /*
        public void InsertElement(string nodeFullPath)
        {
            this.Document.InsertElement(nodeFullPath);
        }
        */

        //public void InsertElement(string nodeFullPath)  //, string StrNome_Atributo = "", string StrValor_Atributo = "")
        //{
        //    XmlElement ObjDomXml_Raiz, ObjDomXml_Elemento, ObjDomXml_Elemento_Aux;
        //    string[] Array_Node;
        //    string StrNomeElemento;
        //    int ShoNx;

        //    ObjDomXml_Elemento = null;
        //    ObjDomXml_Elemento_Aux = null;

        //    Array_Node = nodeFullPath.SplitText("|");

        //    try
        //    {
        //        if (Array_Node.Length == 1)
        //        {
        //            ObjDomXml_Raiz = this.Document.CreateElement(nodeFullPath);

        //            /*
        //            if (StrNome_Atributo.Trim() != "")
        //            {
        //                ObjDomXml_Raiz.SetAttribute(StrNome_Atributo, StrValor_Atributo);
        //            }
        //            */

        //            this.Document.AppendChild(ObjDomXml_Raiz);
        //        }
        //        else
        //        {
        //            for (ShoNx = 0; ShoNx <= Array_Node.Length - 2; ShoNx++)
        //            {
        //                if (ObjDomXml_Elemento.IsNull())
        //                {
        //                    ObjDomXml_Elemento = this.Document.SelectSingleNode(Array_Node[ShoNx]) as XmlElement;
        //                }
        //                else
        //                {
        //                    ObjDomXml_Elemento = ObjDomXml_Elemento.SelectSingleNode(Array_Node[ShoNx]) as XmlElement;
        //                }

        //                if (ObjDomXml_Elemento.Name.Trim() == Array_Node[Array_Node.Length - 2])
        //                {
        //                    StrNomeElemento = Array_Node.LastOrDefault();

        //                    ObjDomXml_Elemento_Aux = this.Document.CreateElement(StrNomeElemento);
        //                    break;
        //                }
        //            }

        //            if (ObjDomXml_Elemento.IsNull())
        //            {
        //                ObjDomXml_Elemento_Aux = this.Document.CreateElement(Array_Node.LastOrDefault());
        //                ObjDomXml_Elemento = this.Document.SelectSingleNode(Array_Node.FirstOrDefault()) as XmlElement;
        //            }

        //            /*
        //            if (StrNome_Atributo.Trim() != "")
        //            {
        //                ObjDomXml_Elemento_Aux.SetAttribute(StrNome_Atributo, StrValor_Atributo.Trim());
        //            }
        //            */

        //            ObjDomXml_Elemento.AppendChild(ObjDomXml_Elemento_Aux);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}


        public void SelectNode(Func<XmlNode, bool> predicate)
        {

        }


        public void InsertElement(string nodeFullPath)  //, string StrNome_Atributo = "", string StrValor_Atributo = "")
        {
            XmlElement elementoRaiz, elementoPai, elementoNovo;
            string[] Array_Node;
            string StrNomeElemento;
            int ShoNx;

            elementoPai = null;
            elementoNovo = null;

            Array_Node = nodeFullPath.SplitText("|");

            try
            {
                if (Array_Node.Length == 1)
                {
                    if (!Document.ContainsTag(Array_Node.First()))
                    {

                        elementoRaiz = Document.CreateElement(nodeFullPath);

                        /*
                        if (StrNome_Atributo.Trim() != "")
                        {
                            ObjDomXml_Raiz.SetAttribute(StrNome_Atributo, StrValor_Atributo);
                        }
                        */

                        Document.AppendChild(elementoRaiz);
                    }
                }
                else
                {
                    for (ShoNx = 0; ShoNx <= Array_Node.Length - 2; ShoNx++)
                    {
                        if (elementoPai.IsNull())
                        {
                            elementoPai = Document.SelectSingleNode(Array_Node[ShoNx]) as XmlElement;

                            if (elementoPai.IsNull())
                                elementoPai = Document.GetTag(Array_Node[ShoNx]) as XmlElement;
                        }
                        else
                        {
                            elementoPai = elementoPai.SelectSingleNode(Array_Node[ShoNx]) as XmlElement;
                        }

                        if (elementoPai.Name.Trim() == Array_Node[Array_Node.Length - 2])
                        {
                            StrNomeElemento = Array_Node.LastOrDefault();

                            elementoNovo = Document.CreateElement(StrNomeElemento);
                            break;
                        }
                    }

                    if (elementoPai.IsNull())
                    {
                        elementoNovo = Document.CreateElement(Array_Node.LastOrDefault());
                        elementoPai = Document.SelectSingleNode(Array_Node.FirstOrDefault()) as XmlElement;
                    }

                    /*
                    if (StrNome_Atributo.Trim() != "")
                    {
                        ObjDomXml_Elemento_Aux.SetAttribute(StrNome_Atributo, StrValor_Atributo.Trim());
                    }
                    */

                    elementoPai.AppendChild(elementoNovo);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void InsertTag(string nodeFullPath, string nodeValue = null, bool formatValue = true)
        {
            string value = null;

            if (nodeValue == null)
            {
                value = (formatValue ? nodeValue.ToNfe() : nodeValue);
            }


            string[] nodes = nodeFullPath.Split(new string[] { "|", "." }, StringSplitOptions.RemoveEmptyEntries);
            XmlNode xmlNode = null;


            for (int idx = 0; idx < nodes.Count(); idx++)
            {
                if (nodes[idx].Trim().IsEmpty())
                {
                    throw new Exception("A estrutura de nodos '" + nodeFullPath + "' não foi informado corretamente! ");
                }
            }

            foreach (string nodeName in nodes)
            {
                XmlNode xmlNodeNew = null;

                if (xmlNode == null)
                {
                    if (Document.DocumentElement.Name == nodeName)
                    {
                        xmlNode = Document.DocumentElement;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    foreach (XmlNode child in xmlNode.ChildNodes)
                    {
                        if (child.Name == nodeName)
                        {
                            xmlNodeNew = child;
                        }
                    }

                    if (xmlNodeNew == null)
                    {
                        XmlElement el = xmlNode.OwnerDocument.CreateElement(nodeName);
                        xmlNodeNew = el;
                        xmlNode.AppendChild(xmlNodeNew);
                        xmlNode = xmlNodeNew;
                    }
                    else
                    {
                        xmlNode = xmlNodeNew;
                    }
                }
            }

            if (xmlNode != null && value != null)
            {
                xmlNode.InnerText = value;
            }
        }

        public void InsertNode(string nodeFullPath, string nodeValue, bool formatValue = true)
        {
            if (nodeValue == null)
            {
                nodeValue = string.Empty;
            }

            if (formatValue)
            {
                InsertNode(Document, nodeFullPath, nodeValue.ToNfe());
            }
            else
            {
                InsertNode(Document, nodeFullPath, nodeValue);
            }
        }

        public void InsertNodeWithCData(string nodeFullPath, string nodeValue)
        {
            if (nodeValue == null)
            {
                nodeValue = string.Empty;
            }

            InsertNode(Document, nodeFullPath, nodeValue, "", "", true);
        }






        //-----------------------------------------------
        /// <summary>
        /// Apenas extende a função [Inserir_XML_No_Node] criada pelo Magno. Não quis perder muito tempo aqui, mas apenas simplificar um pouco.
        /// </summary>    
        private void InsertNode(System.Xml.XmlDocument doc, string nodeName, string nodeValue = "", string attributeName = "", string attributeValue = "", bool isCDataValue = false)
        {
            //NfeVbDll.LegadoMod.InsertNode(ref doc, nodeName, nodeValue, attributeName, attributeValue);
            Inserir_XML_No_Node(doc, nodeName, nodeValue, attributeName, attributeValue, isCDataValue);
        }

        /// <summary>
        /// Apenas extende a função [Inserir_XML_No_Node] criada pelo Magno. Não quis perder muito tempo aqui, mas apenas simplificar um pouco.
        /// </summary>    
        private void InsertNode(System.Xml.XmlDocument doc, string nodeName, object nodeValue, string attributeName = "", string attributeValue = "", bool isCDataValue = false)
        {
            if (nodeValue.GetType() == typeof(Enum))
            {
                Inserir_XML_No_Node(doc, nodeName, nodeValue.GetHashCode().ToString(), attributeName, attributeValue, isCDataValue);
            }
            else
            {
                Inserir_XML_No_Node(doc, nodeName, nodeValue.ToString(), attributeName, attributeValue, isCDataValue);
            }
        }

        /// <summary>
        /// Apenas extende a função [Inserir_XML_No_Node] criada pelo Magno. Não quis perder muito tempo aqui, mas apenas simplificar um pouco.
        /// </summary>
        private void Inserir_XML_No_Node(XmlDocument doc, string StrNome_Node, string StrValor_Node = "", string StrNome_Atributo = "", string StrValor_Atributo = "", bool isCDataValue = false)
        {
            XmlNode nodeNovo, nodePai;
            XmlAttribute XmlDom_Atribute_Node;
            string[] Array_Node;
            int ShoNx;

            nodePai = null;

            if (StrValor_Node == null)
                StrValor_Node = "";

            if (StrNome_Atributo == null)
                StrNome_Atributo = "";

            if (StrValor_Atributo == null)
                StrValor_Atributo = "";

            Array_Node = StrNome_Node.SplitText("|");

            for (int idx = 0; idx < Array_Node.Count(); idx++)
            {
                if (Array_Node[idx].Trim().IsEmpty())
                {
                    throw new Exception("A estrutura de nodos '" + StrNome_Node + "' não foi informado corretamente! ");
                }
            }

            for (ShoNx = 0; ShoNx <= Array_Node.Length - 2; ShoNx++)
            {
                if (nodePai == null)
                {
                    //XmlDom_Node_Aux = doc.SelectSingleNode(Array_Node[ShoNx]) as XmlNode;
                    nodePai = doc.SelectNodes(Array_Node[ShoNx]).LastOrDefault();

                    if (nodePai.IsNull())
                        nodePai = doc.GetTags(Array_Node[ShoNx]).LastOrDefault();
                }
                else
                {
                    //XmlDom_Node_Aux = XmlDom_Node_Aux.SelectSingleNode(Array_Node[ShoNx]);
                    XmlNode nodePaiTemp = nodePai.SelectNodes(Array_Node[ShoNx]).LastOrDefault();

                    if (nodePaiTemp.IsNull())
                        nodePaiTemp = nodePai.GetTags(Array_Node[ShoNx]).LastOrDefault();

                    nodePai = nodePaiTemp;
                }
            }


            nodeNovo = doc.CreateNode(XmlNodeType.Element, Array_Node.Last(), "");

            if (!StrNome_Atributo.IsNullOrEmpty())
            {
                XmlDom_Atribute_Node = doc.CreateAttribute(StrNome_Atributo);
                XmlDom_Atribute_Node.Value = StrValor_Atributo;
                nodeNovo.Attributes.Append(XmlDom_Atribute_Node);
            }

            if (!StrValor_Node.IsNullOrEmpty())
            {
                if (isCDataValue)
                {
                    XmlCDataSection cDataSelection = doc.CreateCDataSection(StrValor_Node.Trim());
                    nodeNovo.AppendChild(cDataSelection);
                }
                else
                {
                    nodeNovo.InnerText = StrValor_Node.Trim();
                }
            }

            nodePai.AppendChild(nodeNovo);
        }




        /*
        /// <summary>
        /// Apenas extende a função [Inserir_XML_No_Elem] criada pelo Magno. Não quis perder muito tempo aqui, mas apenas simplificar um pouco.
        /// </summary>
        /// <param name="SelfObj"></param>
        /// <param name="StrNome_Node"></param>
        /// <param name="StrNome_Atributo"></param>
        /// <param name="StrValor_Atributo"></param>
        public static void InsertElement(this System.Xml.XmlDocument doc, string nodeName, string attributeName = "", string attributeValue = "")
        {
            NfeVbDll.LegadoMod.InsertElement(ref doc, nodeName, attributeName, attributeValue);
        }
        */


        /// <summary>
        /// Apenas extende a função [InsertAttribute] criada pelo Magno. Não quis perder muito tempo aqui, mas apenas simplificar um pouco.
        /// </summary>
        private void InsertAttribute(System.Xml.XmlDocument doc, string nodeName, string attributeName = "", string attributeValue = "")
        {
            //NfeVbDll.LegadoMod.InsertAttribute(ref doc, nodeName, attributeName, attributeValue);
            InsertAttribute2(doc, nodeName, attributeName, attributeValue);
        }



        // Traduzido do VB de: NfeVbDll.LegadoMod.InsertAttribute
        private void InsertAttribute2(System.Xml.XmlDocument SelfObj,
                                   String StrNome_Node,
                                    String nomeDoAtributo = "",
                                    String valorDoAtributo = "")
        {
            XmlElement el = null;
            List<String> listaHierarquia = StrNome_Node.Split(new String[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList();


            foreach (string nomeDoNó in listaHierarquia)
            {
                if (el == null)
                {
                    el = SelfObj.SelectSingleNode(nomeDoNó) as XmlElement;
                }
                else
                {
                    el = el.SelectSingleNode(nomeDoNó) as XmlElement;
                }
            }

            el.SetAttribute(nomeDoAtributo, valorDoAtributo);
        }
        //-------------------------------------------









        private void InsertNodeX(string nodeFullPath, string nodeValue, bool formatValue = true)
        {
            if (nodeValue == null)
            {
                nodeValue = string.Empty;
            }

            if (formatValue)
            {
                nodeValue = nodeValue.ToNfe();
            }


            string[] nodes = nodeFullPath.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            XmlNode xmlNode = null;
            bool encontrouNodeRaiz = false;

            for (int idx = 0; idx < nodes.Count(); idx++)
            {
                if (nodes[idx].Trim().IsEmpty())
                {
                    throw new Exception("A estrutura de nodos '" + nodeFullPath + "' não foi informado corretamente! ");
                }
            }

            foreach (string node in nodes)
            {
                if (!encontrouNodeRaiz)
                {
                    xmlNode = Document.SelectSingleNode(node);
                    encontrouNodeRaiz = true;
                }
                else if (encontrouNodeRaiz && xmlNode == null)
                {
                    break;
                }
                else
                {
                    XmlNodeList nodeList = xmlNode.SelectNodes(node);
                    xmlNode = nodeList.LastOrDefault();
                    //xmlNode = xmlNode.SelectSingleNode(node);
                }
            }

            if (xmlNode != null)
            {
                ////Guid guid = Guid.NewGuid();
                ////string searchKey = "{" + guid.ToString() + "}";


                ////xmlNode.InnerXml = xmlNode.InnerXml + xml.Document.DocumentElement.OuterXml;

                //XmlNode importedNode = xmlNode.OwnerDocument.ImportNode((XmlNode)xml.Document.DocumentElement, true);
                //xmlNode.AppendChild(importedNode);

                ////string newXml = this.Document.OuterXml.Replace(searchKey, xml.Document.DocumentElement.OuterXml);
                ////this.Document.LoadXml(newXml);



                XmlElement el = xmlNode.OwnerDocument.CreateElement(nodes.LastOrDefault());
                XmlNode xmlNodeNew = el;
                xmlNode.AppendChild(xmlNodeNew);
                xmlNode = xmlNodeNew;
            }
        }


        public void InsertNode(string nodeFullPath, object nodeValue)
        {
            string finalValue = string.Empty;

            if (nodeValue.IsEnum())
            {
                finalValue = (nodeValue as Enum).GetHashCode().ToString(); ;
            }
            else
            {
                finalValue = nodeValue.ToString();
            }

            InsertNode(nodeFullPath, finalValue);
        }

        public void InsertNode(string nodeFullPath, Enum nodeValue)
        {
            string finalValue = string.Empty;

            if (nodeValue != null)
            {
                finalValue = nodeValue.GetHashCode().ToString();
            }

            InsertNode(nodeFullPath, finalValue);
        }

        public void InsertNode(string nodeFullPath, decimal nodeValue)
        {
            InsertNode(nodeFullPath, nodeValue.ToSql());
        }

        public void InsertNode(string nodeFullPath, Xml xml)
        {
            string[] nodes = nodeFullPath.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            XmlNode xmlNode = null;

            for (int idx = 0; idx < nodes.Count(); idx++)
            {
                if (nodes[idx].Trim().IsEmpty())
                {
                    throw new Exception("A estrutura de nodos '" + nodeFullPath + "' não foi informado corretamente! ");
                }
            }

            foreach (string node in nodes)
            {
                if (xmlNode == null)
                {
                    xmlNode = Document.SelectSingleNode(node);
                }
                else
                {
                    xmlNode = xmlNode.SelectSingleNode(node);
                }
            }

            if (xmlNode != null)
            {
                //Guid guid = Guid.NewGuid();
                //string searchKey = "{" + guid.ToString() + "}";


                //xmlNode.InnerXml = xmlNode.InnerXml + xml.Document.DocumentElement.OuterXml;

                XmlNode importedNode = xmlNode.OwnerDocument.ImportNode(xml.Document.DocumentElement, true);
                xmlNode.AppendChild(importedNode);

                //string newXml = this.Document.OuterXml.Replace(searchKey, xml.Document.DocumentElement.OuterXml);
                //this.Document.LoadXml(newXml);
            }
        }

        public void InsertNode(string nodeFullPath, XmlElement xml)
        {
            string[] nodes = nodeFullPath.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
            XmlNode xmlNode = null;

            for (int idx = 0; idx < nodes.Count(); idx++)
            {
                if (nodes[idx].Trim().IsEmpty())
                {
                    throw new Exception("A estrutura de nodos '" + nodeFullPath + "' não foi informado corretamente! ");
                }
            }

            foreach (string node in nodes)
            {
                if (xmlNode == null)
                {
                    xmlNode = Document.SelectSingleNode(node);
                }
                else
                {
                    xmlNode = xmlNode.SelectSingleNode(node);
                }
            }

            if (xmlNode != null)
            {
                //Guid guid = Guid.NewGuid();
                //string searchKey = "{" + guid.ToString() + "}";


                //xmlNode.InnerXml = xmlNode.InnerXml + xml.Document.DocumentElement.OuterXml;

                XmlNode importedNode = xmlNode.OwnerDocument.ImportNode(xml, true);
                xmlNode.AppendChild(importedNode);

                //string newXml = this.Document.OuterXml.Replace(searchKey, xml.Document.DocumentElement.OuterXml);
                //this.Document.LoadXml(newXml);
            }
        }

        /*
        private XmlNode ToNode(XmlDocument doc)
        {
            //XmlElement element = new XmlElement();

            
            
            //element.Name = doc.DocumentElement.Name;

        }

        private XmlNode ToNode(XmlNode node)
        {
            XmlElement element = new XmlElement();
            //node.chil

        }
        */

        public XmlNode ToXmlNode()
        {
            return Document.LastChild;

        }

        /// <summary>
        /// Se não for nulo ou empty, insere o node, caso contrário, não incluirá nenhum node (mesmo um vazio <node /> ).
        /// </summary>
        public void InsertNodeIfExists(string nodeFullPath, object nodeValue)
        {
            string finalValue = null;

            if (nodeValue == null)
            {
                return;
            }
            else if (nodeValue.IsEnum())
            {
                finalValue = (nodeValue as Enum).GetHashCode().ToString(); ;
            }
            else if (nodeValue.IsNumeric())
            {
                if (decimal.Parse(nodeValue.ToString()) == 0)
                {
                    return;
                }

                finalValue = nodeValue.ToString();
            }
            else if (string.IsNullOrEmpty(nodeValue.ToString()))
            {
                return;
            }
            else
            {
                finalValue = nodeValue.ToString();
            }

            InsertNode(nodeFullPath, finalValue);
        }

        /// <summary>
        /// Se não for nulo ou empty, insere o node, caso contrário, não incluirá nenhum node (mesmo um vazio <node /> ).
        /// </summary>
        public void InsertNodeIfExists(string nodeFullPath, decimal? nodeValue)
        {
            if (nodeValue == null)
            {
                return;
            }
            else if (nodeValue.IsNumeric())
            {
                if (decimal.Parse(nodeValue.Value.ToString()) == 0)
                {
                    return;
                }
            }
            else if (nodeValue.ToString().IsEmpty())
            {
                return;
            }

            InsertNode(nodeFullPath, nodeValue.Value.ToSql());
        }

        /*
        public void InsertNodeIfExists(string nodeFullPath, PDV.Models.SchemasSefaz.NFSe.Tipos.Simples.TsValor? nodeValue)
        {
            if (nodeValue == null)
            {
                return;
            }

            this.InsertNode(nodeFullPath, nodeValue.ToString().Replace());
        }
        */

        public void InsertAttribute(string nodeFullPath, string attributeName = "", string attributeValue = "")
        {
            if (!string.IsNullOrEmpty(attributeName))
            {
                InsertAttribute(Document, nodeFullPath, attributeName, attributeValue);
            }
        }

        public void InsertAttribute(string nodeFullPath, string attributeName, object attributeValue)
        {
            if (!string.IsNullOrEmpty(attributeName))
            {
                InsertAttribute(Document, nodeFullPath, attributeName, attributeValue.ToString());
            }
        }

        private string IgnoreWhitespaces(string xml)
        {
            return System.Text.RegularExpressions.Regex.Replace(xml, @"(?<=>)[\s ]+(?=<)", "");
        }

        public void Save(string path)
        {
            string xmlText = "";

            if (IgnoreWhitespacesWhenSaveOrLoad)
            {
                xmlText = IgnoreWhitespaces(ToString());
            }
            else
            {
                xmlText = ToString();
            }

            using (System.IO.StreamWriter writer = System.IO.File.CreateText(path))
            {
                writer.Write(xmlText);  //--> substitui caracteres especiais entre tags e grava em arquivo;
            }
        }

        public void Load(string path)
        {
            string xmlText = System.IO.File.ReadAllText(path);

            if (IgnoreWhitespacesWhenSaveOrLoad)
            {
                xmlText = IgnoreWhitespaces(xmlText);
            }

            Document = new XmlDocument();
            Document.PreserveWhitespace = false;
            Document.LoadXml(xmlText);
        }

        public override string ToString()
        {
            //return Xml.IgnoreSpacesAndSpecialCaractersOuterTags(this.Document.OuterXml);
            return Document.OuterXml;
        }



        /// <summary>
        /// Assina digitalmente um documento xml.
        /// </summary>
        /// <param name="xml">Documento a ser assinado</param>
        /// <param name="toSignTagParents">Tag pai da tag que será assinada</param>
        /// <param name="toSignTag">Tag que será assinada</param>
        /// <param name="x509Cert">O certificado digital</param>
        private static void SignXml(XmlDocument xml, string toSignTagParents, string toSignTag, X509Certificate2 x509Cert)
        {
            // Create a new XML document.
            XmlDocument doc = xml;

            // Format the document to ignore white spaces.
            doc.PreserveWhitespace = false;


            if (doc.GetElementsByTagName(toSignTagParents).Count == 0)
            {
                throw new Exception("A tag de assinatura " + toSignTagParents.Trim() + " não existe no XML. (Código do Erro: 5)");
            }
            else if (doc.GetElementsByTagName(toSignTag).Count == 0)
            {
                throw new Exception("A tag de assinatura " + toSignTag.Trim() + " não existe no XML. (Código do Erro: 4)");
            }
            else
            {
                XmlNodeList lists = doc.GetElementsByTagName(toSignTagParents);

                foreach (XmlNode nodes in lists)
                {
                    foreach (XmlNode childNodes in nodes.ChildNodes)
                    {
                        if (!childNodes.Name.Equals(toSignTag))
                            continue;

                        if (childNodes.NextSibling != null && childNodes.NextSibling.Name.Equals("Signature"))
                            continue;

                        // Create a reference to be signed
                        Reference reference = new Reference();
                        reference.Uri = "";

                        XmlElement childElemen = (XmlElement)childNodes;

                        if (childElemen.GetAttributeNode("Id") != null)
                        {
                            //throw new Exception("O nome do atributo [Id] deve estar em letras minúsculas ([id])!");
                            //reference.Uri = ""; // "#" + childElemen.GetAttributeNode("Id").Value;

                            reference.Uri = "#" + childElemen.GetAttributeNode("Id").Value;
                        }
                        else if (childElemen.GetAttributeNode("iD") != null)
                        {
                            //throw new Exception("O nome do atributo [iD] deve estar em letras minúsculas ([id])!");
                            //reference.Uri = ""; // "#" + childElemen.GetAttributeNode("Id").Value;

                            reference.Uri = "#" + childElemen.GetAttributeNode("iD").Value;
                        }
                        else if (childElemen.GetAttributeNode("ID") != null)
                        {
                            //throw new Exception("O nome do atributo [ID] deve estar em letras minúsculas ([id])!");
                            //reference.Uri = ""; // "#" + childElemen.GetAttributeNode("Id").Value;

                            reference.Uri = "#" + childElemen.GetAttributeNode("ID").Value;
                        }
                        else if (childElemen.GetAttributeNode("id") != null)
                        {
                            reference.Uri = "#" + childElemen.GetAttributeNode("id").Value;
                        }
                        else
                        {
                            continue;
                        }

                        reference.DigestMethod = SignedXml.XmlDsigSHA1Url;

                        // Create a SignedXml object.
                        SignedXml signedXml = new SignedXml(doc);

                        // Add the key to the SignedXml document
                        signedXml.SigningKey = x509Cert.PrivateKey;
                        signedXml.SignedInfo.SignatureMethod = SignedXml.XmlDsigRSASHA1Url;


                        // Add an enveloped transformation to the reference.
                        XmlDsigEnvelopedSignatureTransform env = new XmlDsigEnvelopedSignatureTransform();
                        reference.AddTransform(env);

                        XmlDsigC14NTransform c14 = new XmlDsigC14NTransform();
                        reference.AddTransform(c14);

                        // Add the reference to the SignedXml object.
                        signedXml.AddReference(reference);

                        // Create a new KeyInfo object
                        KeyInfo keyInfo = new KeyInfo();

                        // Load the certificate into a KeyInfoX509Data object
                        // and add it to the KeyInfo object.
                        keyInfo.AddClause(new KeyInfoX509Data(x509Cert));

                        // Add the KeyInfo object to the SignedXml object.
                        signedXml.KeyInfo = keyInfo;
                        signedXml.ComputeSignature();

                        // Get the XML representation of the signature and save
                        // it to an XmlElement object.
                        XmlElement xmlDigitalSignature = signedXml.GetXml();

                        nodes.AppendChild(doc.ImportNode(xmlDigitalSignature, true));
                    }
                }
            }
        }



        /*
        /// <summary>
        /// Assina digitalmente um objecto Xml.
        /// </summary>
        /// <param name="xml">Documento a ser assinado</param>
        /// <param name="toSignTagParents">Tag pai da tag que será assinada</param>
        /// <param name="toSignTag">Tag que será assinada</param>
        /// <param name="x509Cert">O certificado digital</param>
        public static void SignXml(Xml xml, string toSignTagParents, string toSignTag, X509Certificate2 x509Cert)
        {
            Xml.SignXml(xml.Document, toSignTagParents, toSignTag, x509Cert);
        }
        */

        /// <summary>
        /// Assina digitalmente um arquivo com formato Xml.
        /// </summary>
        /// <param name="file">Caminho do arquivo xml a ser assinado</param>
        /// <param name="toSignTagParents">Tag pai da tag que será assinada</param>
        /// <param name="toSignTag">Tag que será assinada</param>
        /// <param name="x509Cert">O certificado digital</param>
        public static void SignXml(string file, string toSignTagParents, string toSignTag, X509Certificate2 x509Cert)
        {
            StreamReader SR = null;

            try
            {
                SR = File.OpenText(file);
                string xmlString = SR.ReadToEnd();
                SR.Close();
                SR = null;

                // Create a new XML document.
                XmlDocument doc = new XmlDocument();

                // Format the document to ignore white spaces.
                doc.PreserveWhitespace = false;

                // Load the passed XML file using it’s name.
                doc.LoadXml(xmlString);


                Xml.SignXml(doc, toSignTagParents, toSignTag, x509Cert);

                XmlDocument XMLDoc;

                XMLDoc = new XmlDocument();
                XMLDoc.PreserveWhitespace = false;
                XMLDoc = doc;

                string signedContent = XMLDoc.OuterXml;

                using (StreamWriter sw = File.CreateText(file))
                {
                    sw.Write(signedContent);
                    sw.Close();
                }
            }
            finally
            {
                if (SR != null)
                    SR.Close();
            }
        }


        /*
        public static string IgnoreSubsequentXmlProlog(string xmlText)
        {
            StringBuilder builder = new StringBuilder("");

            for (int idx = 0; idx < xmlText.Length; idx++)
            {

            }

            while (xmlText.IndexOf("<?xml", 0))
            {

            }

            return builder.ToString();
        }
        */



        /*
        /// <summary>
        /// Remove qualquer caracter fora das tags (<...>).
        /// </summary>
        /// <param name="xmlDoc"></param>
        public static string IgnoreSpacesAndSpecialCaractersOuterTags(string xmlText, bool validarXml = true)
        {
            StringBuilder builder = new StringBuilder("");
            bool isTagOpen = false;
            string xml = null;

            if (validarXml)
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.PreserveWhitespace = false;
                xmlDoc.LoadXml(xmlText);    // --> apenas para validar!
                xml = xmlDoc.OuterXml.Trim();
            }
            else
            {
                xml = xmlText;
            }

            List<string> blocks = new List<string>();

            foreach (char x in xml)
            {
                if (char.IsControl(x))
                {
                    continue;
                }

                if (!isTagOpen && x == '<')
                {
                    if (builder.Length > 0)
                    {
                        blocks.Add(builder.ToString());
                        builder.Clear();
                    }

                    isTagOpen = true;
                    builder.Append(x);
                    continue;
                }

                if (isTagOpen && x == '>')
                {
                    isTagOpen = false;
                    builder.Append(x);

                    if (builder.Length > 0)
                    {
                        blocks.Add(builder.ToString());
                        builder.Clear();
                    }

                    continue;
                }

                builder.Append(x);
            }

            if (builder.Length > 0)
            {
                blocks.Add(builder.ToString());
            }

            builder.Clear();

            foreach (string block in blocks)
            {
                builder.Append(block.Trim());
            }

            return builder.ToString();
        }
        */
    }
}