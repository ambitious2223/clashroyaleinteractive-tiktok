using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ClashRoyale.Extensions.Utils;
using ClashRoyale.Logic.Battle;
using ClashRoyale.Logic.Home.Decks.Items;
using ClashRoyale.Protocol.Commands.Server;
using ClashRoyale.Utilities.Netty;
using DotNetty.Buffers;
using Newtonsoft.Json;

namespace ClashRoyale.Core.Network
{
    public class SpawnHttpServer
    {
        private HttpListener _listener;
        private CancellationTokenSource _cts;

        public void Start(int port = 8080)
        {
            _cts = new CancellationTokenSource();
            _listener = new HttpListener();
            _listener.Prefixes.Add($"http://+:{port}/");
            _listener.Prefixes.Add($"http://127.0.0.1:{port}/");

            try
            {
                _listener.Start();
            }
            catch (HttpListenerException)
            {
                _listener = new HttpListener();
                _listener.Prefixes.Add($"http://127.0.0.1:{port}/");
                _listener.Start();
            }

            Logger.Log($"[SpawnHTTP] Listening on http://127.0.0.1:{port}/", GetType());
            Logger.Log($"[SpawnHTTP] POST /spawn  GET /battles  GET /cards", GetType());

            Task.Run(() => AcceptLoop(_cts.Token));
        }

        public void Stop()
        {
            _cts?.Cancel();
            _listener?.Stop();
        }

        private async Task AcceptLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    var ctx = await _listener.GetContextAsync();
                    _ = Task.Run(() => HandleRequest(ctx));
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex)
                {
                    Logger.Log($"[SpawnHTTP] Accept error: {ex.Message}", GetType());
                }
            }
        }

        private void HandleRequest(HttpListenerContext ctx)
        {
            var req = ctx.Request;
            var resp = ctx.Response;

            try
            {
                Logger.Log($"[SpawnHTTP] {req.HttpMethod} {req.Url.AbsolutePath}", GetType());

                if (req.Url.AbsolutePath == "/spawn" && req.HttpMethod == "POST")
                    HandleSpawn(req, resp);
                else if (req.Url.AbsolutePath == "/battles" && req.HttpMethod == "GET")
                    HandleBattles(req, resp);
                else if (req.Url.AbsolutePath == "/cards" && req.HttpMethod == "GET")
                    HandleCards(req, resp);
                else if (req.Url.AbsolutePath == "/config" && req.HttpMethod == "GET")
                    HandleConfig(req, resp);
                else
                    RespondJson(resp, 404, new { error = "Not found" });
            }
            catch (Exception ex)
            {
                Logger.Log($"[SpawnHTTP] Error: {ex.Message}", GetType());
                RespondJson(resp, 500, new { error = ex.Message });
            }
            finally
            {
                resp.Close();
            }
        }

        private void HandleSpawn(HttpListenerRequest req, HttpListenerResponse resp)
        {
            string body;
            using (var reader = new System.IO.StreamReader(req.InputStream, req.ContentEncoding))
                body = reader.ReadToEnd();

            var spawn = JsonConvert.DeserializeObject<SpawnRequest>(body);
            if (spawn == null)
            {
                RespondJson(resp, 400, new { error = "Invalid JSON body" });
                return;
            }

            var battles = Resources.Battles;
            if (battles.Count == 0)
            {
                RespondJson(resp, 400, new { error = "No active battles. Start a battle first." });
                return;
            }

            LogicBattle battle = null;
            if (spawn.battle_id != 0)
                battles.TryGetValue(spawn.battle_id, out battle);

            if (battle == null)
                battle = GetFirstBattle(battles);

            if (battle == null || battle.Count < 2)
            {
                RespondJson(resp, 400, new { error = "Battle not ready (need 2 players)" });
                return;
            }

            var classId = spawn.class_id;
            var instanceId = spawn.instance_id;
            var level = spawn.level <= 0 ? 13 : spawn.level;
            var x = spawn.x;
            var y = spawn.y;
            var team = spawn.team;

            if (classId == 0 && instanceId == 0 && !string.IsNullOrEmpty(spawn.troop_id))
            {
                var cardKey = TroopCardMap.Find(spawn.troop_id);
                if (cardKey != null)
                {
                    classId = cardKey.Value.classId;
                    instanceId = cardKey.Value.instanceId;
                }
                else
                {
                    RespondJson(resp, 400, new
                    {
                        error = $"Unknown troop_id: {spawn.troop_id}",
                        hint = "Use troop_id like 'knight', 'giant', 'archers' or class_id+instance_id"
                    });
                    return;
                }
            }

            var senderPlayer = team == 0 ? battle[0] : battle[1];
            var senderHighId = senderPlayer.Home.HighId;
            var senderLowId = senderPlayer.Home.LowId;

            var cmd = new DoSpellCommand(null, null)
            {
                ClientTick = 100,
                Checksum = 0,
                SenderHighId = senderHighId,
                SenderLowId = senderLowId,
                SpellDeckIndex = 0,
                ClassId = classId,
                InstanceId = instanceId,
                SpellIndex = 0,
                TroopLevel = level,
                X = x,
                Y = y
            };

            var cmdData = BuildCommandBytes(cmd);

            var ownQueue = battle.GetOwnQueue(senderPlayer.Home.Id);
            ownQueue.Enqueue(cmdData);

            var enemyBuffer = Unpooled.Buffer(9);
            enemyBuffer.WriteBytes(cmdData);
            enemyBuffer.WriteVInt(1);
            enemyBuffer.WriteVInt(Card.Id(classId, instanceId));
            enemyBuffer.WriteVInt(level);
            enemyBuffer.WriteVInt(x);
            enemyBuffer.WriteVInt(y);

            if (battle.Is2V2)
            {
                foreach (var queue in battle.GetOtherQueues(senderPlayer.Home.Id))
                    queue.Enqueue(enemyBuffer.Array);
            }
            else
            {
                var enemyQueue = battle.GetEnemyQueue(senderPlayer.Home.Id);
                if (enemyQueue != null)
                    enemyQueue.Enqueue(enemyBuffer.Array);
            }

            Logger.Log(
                $"[SpawnHTTP] Spawned {Card.Id(classId, instanceId)} (Lvl{level}) at ({x},{y}) team={team} in Battle#{battle.BattleId}",
                GetType());

            RespondJson(resp, 200, new
            {
                ok = true,
                battle_id = battle.BattleId,
                card = Card.Id(classId, instanceId),
                class_id = classId,
                instance_id = instanceId,
                level = level,
                x = x,
                y = y,
                team = team
            });
        }

        private byte[] BuildCommandBytes(DoSpellCommand cmd)
        {
            var buffer = Unpooled.Buffer(64);
            try
            {
                buffer.WriteVInt(cmd.Type);
                buffer.WriteVInt(cmd.ClientTick);
                buffer.WriteVInt(cmd.Checksum);
                buffer.WriteVInt(cmd.SenderHighId);
                buffer.WriteVInt(cmd.SenderLowId);
                buffer.WriteVInt(cmd.SpellDeckIndex);
                buffer.WriteVInt(cmd.ClassId);
                buffer.WriteVInt(cmd.InstanceId);
                buffer.WriteVInt(cmd.SpellIndex);
                buffer.WriteVInt(cmd.TroopLevel);
                buffer.WriteVInt(cmd.X);
                buffer.WriteVInt(cmd.Y);

                var bytes = new byte[buffer.ReadableBytes];
                buffer.ReadBytes(bytes);
                return bytes;
            }
            finally
            {
                buffer.Release();
            }
        }

        private void HandleBattles(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var battles = Resources.Battles;
            var list = new List<object>();

            foreach (var kv in battles)
            {
                var b = kv.Value;
                var players = new List<string>();
                foreach (var p in b)
                    if (p != null) players.Add(p.Home.Name);

                list.Add(new
                {
                    battle_id = kv.Key,
                    players = players,
                    is_running = b.IsRunning,
                    is_2v2 = b.Is2V2,
                    is_friendly = b.IsFriendly,
                    seconds = b.BattleSeconds
                });
            }

            RespondJson(resp, 200, new { battles = list, count = list.Count });
        }

        private void HandleCards(HttpListenerRequest req, HttpListenerResponse resp)
        {
            var cards = TroopCardMap.GetAll();
            RespondJson(resp, 200, new { cards = cards });
        }

        private void HandleConfig(HttpListenerRequest req, HttpListenerResponse resp)
        {
            RespondJson(resp, 200, new
            {
                server_port = Resources.Configuration.ServerPort,
                starting_elixir = 10,
                elixir_regen_ms = 280,
                message = "Elixir is unlimited. Spawn freely."
            });
        }

        private static void RespondJson(HttpListenerResponse resp, int code, object obj)
        {
            resp.StatusCode = code;
            resp.ContentType = "application/json";
            var json = JsonConvert.SerializeObject(obj);
            var bytes = Encoding.UTF8.GetBytes(json);
            resp.ContentLength64 = bytes.Length;
            resp.OutputStream.Write(bytes, 0, bytes.Length);
        }

        private static LogicBattle GetFirstBattle(Database.Cache.Battles battles)
        {
            foreach (var kv in battles)
                if (kv.Value.Count >= 2)
                    return kv.Value;
            return null;
        }

        public static class TroopCardMap
        {
            private static readonly Dictionary<string, (int classId, int instanceId)> Map =
                new Dictionary<string, (int, int)>(StringComparer.OrdinalIgnoreCase)
                {
                    {"knight", (26, 0)}, {"archers", (26, 1)}, {"goblins", (26, 2)},
                    {"giant", (26, 3)}, {"pekka", (26, 4)}, {"minions", (26, 5)},
                    {"balloon", (26, 6)}, {"witch", (26, 7)}, {"barbarians", (26, 8)},
                    {"golem", (26, 9)}, {"skeletons", (26, 10)}, {"valkyrie", (26, 11)},
                    {"skeleton army", (26, 12)}, {"skeletonarmy", (26, 12)},
                    {"bomb tower", (26, 13)}, {"bomber", (26, 14)}, {"musketeer", (26, 15)},
                    {"mini pekka", (26, 16)}, {"minipekka", (26, 16)}, {"prince", (26, 17)},
                    {"wizard", (26, 18)}, {"minion horde", (26, 19)}, {"minionhorde", (26, 19)},
                    {"hog rider", (26, 20)}, {"hogrider", (26, 20)},
                    {"dark prince", (26, 21)}, {"darkprince", (26, 21)},
                    {"bowler", (26, 22)}, {"baby dragon", (26, 23)}, {"babydragon", (26, 23)},
                    {"miner", (26, 24)}, {"sparky", (26, 25)}, {"ice wizard", (26, 26)},
                    {"icewizard", (26, 26)}, {"princess", (26, 27)}, {"lavahound", (26, 28)},
                    {"lava hound", (26, 28)}, {"inferno dragon", (26, 29)},
                    {"electro wizard", (26, 30)}, {"electrowizard", (26, 30)},
                    {"graveyard", (26, 31)}, {"mega minion", (26, 32)}, {"megaminion", (26, 32)},
                    {"dart goblin", (26, 33)}, {"dartgoblin", (26, 33)},
                    {"goblin gang", (26, 34)}, {"goblingang", (26, 34)},
                    {"bandit", (26, 35)}, {"night witch", (26, 36)}, {"nightwitch", (26, 36)},
                    {"bats", (26, 37)}, {"royal ghost", (26, 38)}, {"rascals", (26, 39)},
                    {"cannon cart", (26, 40)}, {"cannoncart", (26, 40)},
                    {"royal hogs", (26, 41)}, {"royalhogs", (26, 41)},
                    {"goblin giant", (26, 42)}, {"goblingiant", (26, 42)},
                    {"skeleton barrel", (26, 43)}, {"skeletonbarrel", (26, 43)},
                    {"flying machine", (26, 44)}, {"flyingmachine", (26, 44)},
                    {"wall breakers", (26, 45)}, {"wallbreakers", (26, 45)},
                    {"royal recruit", (26, 46)}, {"royalrecruit", (26, 46)},
                    {"zappies", (26, 47)}, {"elixir golem", (26, 48)}, {"elixirgolem", (26, 48)},
                    {"spear goblins", (26, 49)}, {"speargoblins", (26, 49)},
                    {"fire spirits", (26, 50)}, {"firespirits", (26, 50)},
                    {"ice spirit", (26, 51)}, {"icespirit", (26, 51)},
                    {"royal giant", (26, 52)}, {"royalgiant", (26, 52)},
                    {"hunter", (26, 53)}, {"barbarian barrel", (26, 54)},
                    {"fisherman", (26, 55)}, {"magic archer", (26, 56)},
                    {"phoenix", (26, 57)}, {"little prince", (26, 58)},

                    {"tesla", (27, 0)}, {"cannon", (27, 1)}, {"mortar", (27, 2)},
                    {"inferno tower", (27, 3)}, {"infernotower", (27, 3)},
                    {"barbarian hut", (27, 4)}, {"bomb tower b", (27, 5)},
                    {"elixir collector", (27, 6)}, {"elixircollector", (27, 6)},
                    {"goblin hut", (27, 7)}, {"furnace", (27, 8)},

                    {"fireball", (28, 0)}, {"arrows", (28, 1)}, {"rage", (28, 2)},
                    {"rocket", (28, 3)}, {"goblin barrel", (28, 4)}, {"goblinbarrel", (28, 4)},
                    {"freeze", (28, 5)}, {"mirror", (28, 6)}, {"lightning", (28, 7)},
                    {"zap", (28, 8)}, {"poison", (28, 9)}, {"tornado", (28, 10)},
                    {"clone", (28, 11)}, {"earthquake", (28, 12)},
                    {"log", (28, 14)}, {"the log", (28, 14)},
                    {"giant snowball", (28, 15)}, {"giantsnowball", (28, 15)},
                    {"royal delivery", (28, 16)}, {"void", (28, 17)},
                };

            public static (int classId, int instanceId)? Find(string name)
            {
                if (Map.TryGetValue(name, out var val))
                    return val;
                return null;
            }

            public static List<object> GetAll()
            {
                var result = new List<object>();
                var seen = new HashSet<string>();
                foreach (var kv in Map)
                {
                    if (!seen.Contains(kv.Key.ToLower()))
                    {
                        result.Add(new { troop_id = kv.Key, class_id = kv.Value.classId, instance_id = kv.Value.instanceId });
                        seen.Add(kv.Key.ToLower());
                    }
                }
                return result;
            }
        }

        public class SpawnRequest
        {
            public string troop_id { get; set; }
            public int class_id { get; set; }
            public int instance_id { get; set; }
            public int level { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public int team { get; set; }
            public long battle_id { get; set; }
        }
    }
}
