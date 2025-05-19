using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroHandlerService.RMQ
{
    public interface IRmqProducer
    {
        public void SendMessage<T>(string messageId, T message) where T : class;
    }
}
