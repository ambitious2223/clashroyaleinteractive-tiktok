using ClashRoyale.Logic;
using ClashRoyale.Protocol.Messages.Server;
using ClashRoyale.Utilities.Netty;
using DotNetty.Buffers;

namespace ClashRoyale.Protocol.Messages.Client.Login
{
    public class ClientHelloMessage : PiranhaMessage
    {
        public ClientHelloMessage(Device device, IByteBuffer buffer) : base(device, buffer)
        {
            Id = 10100;
            RequiredState = Device.State.Disconnected;
        }

        public int Protocol { get; set; }
        public int KeyVersion { get; set; }
        public int MajorVersion { get; set; }
        public int MinorVersion { get; set; }
        public int Build { get; set; }
        public string FingerprintSha { get; set; }
        public int DeviceType { get; set; }
        public int AppStore { get; set; }

        public override void Decrypt()
        {
            // already decrypted
        }

        public override void Decode()
        {
            Protocol = Reader.ReadInt();
            KeyVersion = Reader.ReadInt();
            MajorVersion = Reader.ReadInt();
            MinorVersion = Reader.ReadInt();
            Build = Reader.ReadInt();
            FingerprintSha = Reader.ReadScString();
            DeviceType = Reader.ReadInt();
            AppStore = Reader.ReadInt();
        }

        public override async void Process()
        {
            // ClientHello is always the first message and is never encrypted.
            Device.CurrentState = Device.State.Login;

            Logger.Log(
                $"ClientHello protocol={Protocol} key={KeyVersion} v={MajorVersion}.{MinorVersion}.{Build} fp={FingerprintSha}",
                GetType());

            // Only kick the client into a content patch if it actually sent a
            // fingerprint and it does not match ours.
            if (Resources.Configuration.UseContentPatch &&
                !string.IsNullOrEmpty(FingerprintSha) &&
                FingerprintSha != Resources.Fingerprint.Sha)
            {
                await new LoginFailedMessage(Device)
                {
                    ErrorCode = 7,
                    ContentUrl = Resources.Configuration.PatchUrl,
                    ResourceFingerprintData = Resources.Fingerprint.Json,
                    SkipCrypto = true
                }.SendAsync();
                return;
            }

            // Standard handshake: answer with ServerHello (20100, plaintext) so the
            // client advances to the Login step. Without this the client waits
            // forever and never sends Login (10101).
            await new ServerHelloMessage(Device).SendAsync();

            Logger.Log("ServerHello (20100) sent; waiting for Login (10101).", GetType());
        }
    }
}