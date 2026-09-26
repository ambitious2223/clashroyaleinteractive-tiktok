using ClashRoyale.Logic;

namespace ClashRoyale.Protocol.Messages.Server
{
    public class ServerHelloMessage : PiranhaMessage
    {
        public ServerHelloMessage(Device device) : base(device)
        {
            Id = 20100;
        }

        public override void Encrypt()
        {
        }

        public override void Encode()
        {
            var sessionKey = new byte[24];
            Writer.WriteInt(sessionKey.Length);
            Writer.WriteBytes(sessionKey);
        }
    }
}
