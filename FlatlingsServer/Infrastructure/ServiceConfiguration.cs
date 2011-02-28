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
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;
using Autofac;
using Microsoft.ServiceModel.Description;
using Microsoft.ServiceModel.Http;

namespace FlatlingsServer.Infrastructure
{
    public class ServiceConfiguration : HttpHostConfiguration, IProcessorProvider, IInstanceFactory
    {
        private readonly IComponentContext _container;

        public ServiceConfiguration(IComponentContext container)
        {
            _container = container;
        }

        public void RegisterRequestProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
        {
            var xmlProcessor = processors.OfType<XmlProcessor>().FirstOrDefault();
            if (xmlProcessor != null)
            {
                processors.Remove(xmlProcessor);
            }

            processors.Add(new DataContractXmlProcessor(operation, mode));
            processors.Add(new JsonProcessor(operation, mode));
            processors.Add(new ProtocolBufferProcessor(operation, mode));
        }

        public void RegisterResponseProcessorsForOperation(HttpOperationDescription operation, IList<Processor> processors, MediaTypeProcessorMode mode)
        {
            var xmlProcessor = processors.OfType<XmlProcessor>().FirstOrDefault();
            if (xmlProcessor != null)
            {
                processors.Remove(xmlProcessor);
            }

            processors.Add(new DataContractXmlProcessor(operation, mode));
            processors.Add(new JsonProcessor(operation, mode));
            processors.Add(new ProtocolBufferProcessor(operation, mode));
        }

        public object GetInstance(Type serviceType, InstanceContext instanceContext, Message message)
        {
            return _container.Resolve(serviceType);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object service)
        {
            
        }
    }
}