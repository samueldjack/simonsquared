using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ProtoBuf;
using ProtoBuf.Meta;
using SimonSquared.Online.DataContracts;

namespace ContractsSerializerCompiler
{
    class Program
    {
        static void Main(string[] args)
        {
            var model = TypeModel.Create();

            model.AutoAddMissingTypes = true;

            var types = from type in typeof (ResponseMessage).Assembly.GetTypes()
                        let attribute = Attribute.GetCustomAttribute(type, typeof(ProtoContractAttribute))
                        where attribute != null
                        select type;

            foreach (var type in types)
            {
                model.Add(type, true);
            }

            model.Compile("DataContractsSerializer", "DataContractsSerializer.dll");

            File.Copy("DataContractsSerializer.dll", "../../../lib/WP7/DataContractsSerializer.dll",true);
        }
    }
}
