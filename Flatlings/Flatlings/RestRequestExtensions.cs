#region License
// This file is part of Simon Squared
// 
// Simon Squared is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Simon Squared is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
// along with Simon Squared. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using RestSharp;
using RestSharp.Serializers;

namespace Flatlings
{
    public static class RestRequestExtensions
    {
        public static RestRequest AddBody<TBaseType>(this RestRequest request, TBaseType instance, IEnumerable<Type> knownTypes)
        {
            request.RequestFormat = DataFormat.Xml;
            request.XmlSerializer = new DataContractXmlSerializer(knownTypes, typeof (TBaseType));
            request.AddBody(instance);

            return request;
        }

        public class DataContractXmlSerializer : ISerializer
        {
            private readonly IEnumerable<Type> _knownTypes;
            private readonly Type _rootType;

            public DataContractXmlSerializer(IEnumerable<Type> knownTypes, Type rootType)
            {
                _knownTypes = knownTypes;
                _rootType = rootType;

                ContentType = "text/xml";
            }

            public string Serialize(object obj)
            {
                var writer = new StringWriter();
                using (var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings()
                                                                    {
                                                                        OmitXmlDeclaration = true,
                                                                        NamespaceHandling =
                                                                            NamespaceHandling.OmitDuplicates,
                                                                        ConformanceLevel = ConformanceLevel.Fragment
                                                                    }))
                {
                    var serializer = new DataContractSerializer(_rootType, _knownTypes);
                    serializer.WriteObject(xmlWriter, obj);
                }

                return writer.ToString();
            }

            public string RootElement
            {
                get; set;
            }

            public string Namespace
            {
                get; set;
            }

            public string DateFormat
            {
                get; set; 
            }

            public string ContentType { get;
                set;
            }
        }
    }
}
