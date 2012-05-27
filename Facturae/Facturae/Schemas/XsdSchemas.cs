﻿using System;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace ElectronicInvoice.Schemas
{
    /// <summary>
    /// Helper class for dealing with Facturae and xmldsig Schemas
    /// </summary>
    internal static class XsdSchemas
    {
        #region · Constants ·

        public const string FacturaeNamespaceUrl    = "http://www.facturae.es/Facturae/2009/v3.2/Facturae";
        public const string XmlDsigNamespaceUrl     = "http://www.w3.org/2000/09/xmldsig#";
        public const string XadesNamespaceUrl       = "http://uri.etsi.org/01903/v1.3.2#";
        public const string FacturaePrefix          = "fe";
        public const string XmlDsigPrefix           = "ds";
        public const string XadesPrefix             = "";

        #endregion

        #region · Members ·

        private static readonly object SyncObject = new object();
        private static XmlSchemaSet FacturaeSchemaSet;

        #endregion

        #region · Static Constructor ·

        static XsdSchemas()
        {
        }

        #endregion

        #region · Methods ·
		
        public static string FormatId(string firstPart)
        {
            return String.Format("{0}-{1}", firstPart, DateTime.Today.ToString("yyyyMMdd"));
        }

        public static string FormatId(string firstPart, string secondPart)
        {
            return String.Format("{0}-{1}{2}", firstPart, secondPart, DateTime.Today.ToString("yyyyMMdd"));
        }
		
        public static string DateTimeToCanonicalRepresentation(DateTime now)
        {
            return now.ToString("yyyy-MM-ddTHH:mm:sszzz");
        }

        public static string NowInCanonicalRepresentation()
        {
            return DateTimeToCanonicalRepresentation(DateTime.Now);
        }		
		
        public static XmlNamespaceManager CreateXadesNamespaceManager(XmlDocument document)
        {
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(document.NameTable);
            
            nsmgr.AddNamespace(XsdSchemas.FacturaePrefix, XsdSchemas.FacturaeNamespaceUrl);
            nsmgr.AddNamespace(XsdSchemas.XmlDsigPrefix , XsdSchemas.XmlDsigNamespaceUrl);
            nsmgr.AddNamespace(XsdSchemas.XadesPrefix   , XsdSchemas.XadesNamespaceUrl);

            return nsmgr;
        }

        public static XmlSerializerNamespaces CreateXadesSerializerNamespace()
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();

            ns.Add(XsdSchemas.FacturaePrefix, XsdSchemas.FacturaeNamespaceUrl);
            ns.Add(XsdSchemas.XmlDsigPrefix , XsdSchemas.XmlDsigNamespaceUrl);

            return ns;
        }

        /// <summary>
        /// From mono SignedXml class
        /// https://github.com/mono/mono/blob/master/mcs/class/System.Security/System.Security.Cryptography.Xml/SignedXml.cs
        /// </summary>
        /// <param name="envDoc"></param>
        /// <param name="inputElement"></param>
        /// <returns></returns>
        public static XmlElement FixupNamespaces(XmlDocument envDoc, XmlElement inputElement)
        {
            XmlDocument doc = new XmlDocument { PreserveWhitespace = true };

            doc.LoadXml(inputElement.OuterXml);

            if (envDoc != null)
            {
                foreach (XmlAttribute attr in envDoc.DocumentElement.SelectNodes("namespace::*"))
                {
                    if (attr.LocalName == "xml")
                    {
                        continue;
                    }
					
                    if (attr.Prefix == doc.DocumentElement.Prefix)
                    {
                        continue;
                    }

                    doc.DocumentElement.SetAttributeNode(doc.ImportNode(attr, true) as XmlAttribute);
                }
            }

            return doc.DocumentElement;
        }		

        /// <summary>
        /// Gets the schema set.
        /// </summary>
        /// <returns>A instance of <see cref="XmlSchemaSet"/></returns>
        public static XmlSchemaSet GetSchemaSet()
        {
            return GetSchemaSet(null);
        }

        /// <summary>
        /// Gets the schema set built with the given xml name table.
        /// </summary>
        /// <param name="nt">The name table.</param>
        /// <returns>A instance of <see cref="XmlSchemaSet"/></returns>
        public static XmlSchemaSet GetSchemaSet(XmlNameTable nt)
        {
            lock (SyncObject)
            {
                if (FacturaeSchemaSet == null)
                {
                    if (nt != null)
                    {
                        FacturaeSchemaSet = new XmlSchemaSet(nt);
                    }
                    else
                    {
                        FacturaeSchemaSet = new XmlSchemaSet();
                    }

                    FacturaeSchemaSet.XmlResolver = new XmlUrlResolver();
                    FacturaeSchemaSet.Add(ReadSchema("ElectronicInvoice.Schemas.xmldsig-core-schema.xsd"));
                    FacturaeSchemaSet.Add(ReadSchema("ElectronicInvoice.Schemas.Facturaev3_2.xsd"));
                    FacturaeSchemaSet.Compile();
                }
            }

            return FacturaeSchemaSet;
        }

        #endregion

        #region · Private Methods ·

        private static XmlSchema ReadSchema(string resourceName)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            return XmlSchema.Read(currentAssembly.GetManifestResourceStream(resourceName), SchemaSetValidationEventHandler);
        }

        private static void SchemaSetValidationEventHandler(object sender, ValidationEventArgs e)
        {
        }

        #endregion
    }
}
