using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NETUploadClient.SyncSocketProtocolCore;

namespace NETUploadClient.SyncSocketProtocol
{
    class ClientControlSocket : ClientBaseSocket
    {
        public ClientControlSocket()
            : base()
        {
            m_protocolFlag = AsyncSocketServer.ProtocolFlag.Control;

        }

        public bool DoGetClients()
        {
            try
            {
                m_outgoingDataAssembler.Clear();
                m_outgoingDataAssembler.AddRequest();
                m_outgoingDataAssembler.AddCommand(AsyncSocketServer.ProtocolKey.GetClients);
                SendCommand();
                byte[] buffer;
                int offset;
                int size;
                bool bSuccess=RecvCommand(out buffer, out offset, out size);
                string data=System.Text.Encoding.UTF8.GetString(buffer,0,offset);
                string str=null;
                m_incomingDataParser.GetValue("item", ref str);

                Console.WriteLine(str);
                Console.WriteLine();
                if (bSuccess)
                {

                    if (CheckErrorCode())
                    {

                        return true;
                    }
                    else return false;
                }
                    
                else
                    return false;
            }
            catch (Exception E)
            {
                //记录日志
                m_errorString = E.Message;
                return false;
            }
            }
            
        }
        
    }
