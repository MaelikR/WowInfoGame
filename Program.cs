using Discord;
using Discord.Commands;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WowGameInfo
{
    internal class Program
    {
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private InteractionService _interactions;

        private static Dictionary<ulong, int> _userPoints = new();
        private const string ScoreFile = "scores.json";

        private static Task Main(string[] args) => new Program().MainAsync();

        public async Task MainAsync()
        {
            LoadScores();

            _client = new DiscordSocketClient(new DiscordSocketConfig { LogLevel = LogSeverity.Info });
            _commands = new CommandService();
            _interactions = new InteractionService(_client.Rest); // ← ✅ ici

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_interactions)
                .BuildServiceProvider();

            _client.Log += LogAsync;
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);

            string token = "Token Discord"; // ⚠️ Ne jamais laisser en clair

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            _client.InteractionCreated += async interaction =>
            {
                var ctx = new SocketInteractionContext(_client, interaction);
                await _interactions.ExecuteCommandAsync(ctx, _services);
            };

            _client.Ready += async () =>
            {
                await _interactions.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
                await _interactions.RegisterCommandsGloballyAsync();
                Console.WriteLine("✅ Slash commands enregistrées !");
            };


            Console.WriteLine("✅ Bot WoWGameInfo prêt !");
            await Task.Delay(-1);
        }

        public class WowAdvancedModule : ModuleBase<SocketCommandContext>
        {
           // [Command("talents")]
            public async Task TalentsAsync([Remainder] string classe)
            {
                string url = $"https://www.wowhead.com/talent-calc/{classe.ToLower()}";
                var embed = new EmbedBuilder()
                    .WithTitle($"🔧 Talents pour {classe}")
                    .WithDescription("Voici l'arbre de talents interactif sur Wowhead.")
                    .WithUrl(url)
                    .WithColor(Color.DarkPurple)
                    .Build();
                await ReplyAsync(embed: embed);
            }

            [Command("build")]
            public async Task BuildAsync([Remainder] string classe)
            {
                string classeLower = classe.ToLower();
                string url = $"https://www.icy-veins.com/wow/{classeLower}-pve-guide";
                var embed = new EmbedBuilder()
                    .WithTitle($"⚔️ Build recommandé : {classe}")
                    .WithDescription("Guide PvE complet par Icy Veins.")
                    .WithUrl(url)
                    .WithColor(Color.DarkBlue)
                    .Build();
                await ReplyAsync(embed: embed);
            }

            [Command("lore")]
            public async Task LoreAsync([Remainder] string perso)
            {
                string lien = $"https://wowpedia.fandom.com/wiki/{Uri.EscapeDataString(perso.Replace(" ", "_"))}";
                var embed = new EmbedBuilder()
                    .WithTitle($"📚 Lore de {perso}")
                    .WithDescription($"Découvre l'histoire de {perso} dans l'univers de WoW.")
                    .WithUrl(lien)
                    .WithColor(Color.Teal)
                    .Build();
                await ReplyAsync(embed: embed);
            }

            [Command("donjons")]
            public async Task DonjonsAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🏰 Donjons populaires de WoW")
                    .WithColor(Color.Gold)
                    .WithDescription("Voici quelques donjons emblématiques :")
                    .AddField("⚒️ Ragefeu", "[Voir](https://www.wowhead.com/zone=2437)")
                    .AddField("❄️ Caveau d’Utgarde", "[Voir](https://www.wowhead.com/zone=206)")
                    .AddField("🐍 Temple du Serpent de Jade", "[Voir](https://www.wowhead.com/zone=9591)")
                    .AddField("🔥 Flèches de Sethekk", "[Voir](https://www.wowhead.com/zone=3791)")
                    .AddField("⚙️ Méchagon", "[Voir](https://www.wowhead.com/zone=1490)");
                await ReplyAsync(embed: embed.Build());
            }

            [Command("raid")]
            public async Task RaidAsync([Remainder] string extension)
            {
                Dictionary<string, List<(string Nom, string Lien)>> raids = new()
                {
                    ["lichking"] = new()
            {
                ("Naxxramas", "https://www.wowhead.com/zone=3456"),
                ("Ulduar", "https://www.wowhead.com/zone=4273"),
                ("Citadelle de la Couronne de glace", "https://www.wowhead.com/zone=4812")
            },
                    ["legion"] = new()
            {
                ("Cauchemar d’émeraude", "https://www.wowhead.com/zone=1520"),
                ("Palais Sacrenuit", "https://www.wowhead.com/zone=1530"),
                ("Tombe de Sargeras", "https://www.wowhead.com/zone=1676")
            },
                    ["bfa"] = new()
            {
                ("Uldir", "https://www.wowhead.com/zone=1861"),
                ("Bataille de Dazar'alor", "https://www.wowhead.com/zone=2070"),
                ("Palais éternel", "https://www.wowhead.com/zone=2164")
            }
                };

                extension = extension.ToLower().Trim();
                if (!raids.ContainsKey(extension))
                {
                    await ReplyAsync("❌ Extension non reconnue. Essaie : `lichking`, `legion`, `bfa`.");
                    return;
                }

                var embed = new EmbedBuilder()
                    .WithTitle($"🏟️ Raids majeurs de {extension}")
                    .WithColor(Color.Red);

                foreach (var (nom, lien) in raids[extension])
                    embed.AddField(nom, $"[Voir sur Wowhead]({lien})");

                await ReplyAsync(embed: embed.Build());
            }
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            if (messageParam is not SocketUserMessage message || message.Author.IsBot) return;

            int argPos = 0;
            if (!message.HasCharPrefix('!', ref argPos)) return;

            var context = new SocketCommandContext(_client, message);
            var result = await _commands.ExecuteAsync(context, argPos, _services);

            if (!result.IsSuccess)
                Console.WriteLine($"❌ Commande échouée : {result.ErrorReason}");
        }

        private Task LogAsync(LogMessage log)
        {
            Console.WriteLine(log.ToString());
            return Task.CompletedTask;
        }

        private void LoadScores()
        {
            if (File.Exists(ScoreFile))
            {
                var json = File.ReadAllText(ScoreFile);
                _userPoints = JsonSerializer.Deserialize<Dictionary<ulong, int>>(json) ?? new();
            }
        }
        public class WowExtraModule : ModuleBase<SocketCommandContext>
        {
            [Command("defi")]
            public async Task DefiAsync()
            {
                string[] defis = {
            "Tuer 10 murlocs sans mourir !",
            "Finir un donjon en moins de 15 minutes.",
            "Faire une emote /dance devant un boss de raid.",
            "Farmer 50 plantes sans monture volante.",
            "Utiliser uniquement les sorts de rang 1 pendant 10 minutes."
        };
                string defi = defis[new Random().Next(defis.Length)];

                var embed = new EmbedBuilder()
                    .WithTitle("🎯 Défi du jour")
                    .WithDescription(defi)
                    .WithColor(Color.Orange)
                    .Build();
                await ReplyAsync(embed: embed);
            }

            [Command("anecdote")]
            public async Task AnecdoteAsync()
            {
                string[] anecdotes = {
            "Le cri de mort des murlocs est devenu un mème légendaire.",
            "Arthas est l’un des personnages les plus tragiques de WoW.",
            "La zone Durotar est nommée d’après le père de Thrall.",
            "Il existe une quête où tu dois parler à une chèvre… et ça marche !"
        };
                var a = anecdotes[new Random().Next(anecdotes.Length)];

                await ReplyAsync($"📚 **Anecdote WoW :** {a}");
            }

            [Command("citation")]
            public async Task CitationAsync()
            {
                var quotes = new[]
                {
            "\"You are not prepared!\" – Illidan Stormrage",
            "\"Arthas, mon fils…\" – Le roi Terenas",
            "\"Lok'tar Ogar!\" – Cri de guerre orc",
            "\"Les vivants ne peuvent vaincre la mort…\" – Le Roi-Liche"
        };
                await ReplyAsync($"🗨️ **Citation :** {quotes[new Random().Next(quotes.Length)]}");
            }

            [Command("astuce")]
            public async Task AstuceAsync()
            {
                string[] astuces = {
            "💡 Utilise `/follow` pour ne jamais perdre ton tank !",
            "💡 Assigne des touches pour marquer les mobs rapidement.",
            "💡 Un bon DPS c'est bien, un DPS vivant c’est mieux.",
            "💡 Ne cours pas devant le tank en donjon."
        };
                await ReplyAsync(astuces[new Random().Next(astuces.Length)]);
            }

            [Command("farm")]
            public async Task FarmAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🌾 Spot de farm recommandé")
                    .WithDescription("📍 Les Hautes-terres Arathies pour l’herboristerie.\n📍 Gorges des Vents brûlants pour le minerai.")
                    .WithUrl("https://www.wow-professions.com/gathering")
                    .WithColor(Color.Green)
                    .Build();
                await ReplyAsync(embed: embed);
            }

            [Command("rotation")]
            public async Task RotationAsync([Remainder] string classe)
            {
                var url = $"https://www.icy-veins.com/wow/{classe.ToLower()}-pve-dps-rotation-cooldowns-abilities";
                var embed = new EmbedBuilder()
                    .WithTitle($"🔁 Rotation DPS - {classe}")
                    .WithDescription("Consulte ta rotation optimale ici :")
                    .WithUrl(url)
                    .WithColor(Color.Blue)
                    .Build();
                await ReplyAsync(embed: embed);
            }

            [Command("quetes")]
            public async Task QuetesAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("📜 Quêtes épiques à faire")
                    .WithDescription("1. La Main de la rédemption\n2. L'ombre d'Arthas\n3. Le retour de Tirion Fordring")
                    .WithColor(Color.DarkTeal)
                    .Build();
                await ReplyAsync(embed: embed);
            }

            [Command("bg")]
            public async Task BgAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("⚔️ Champs de bataille")
                    .WithDescription("📍 Goulet des Chanteguerres\n📍 Vallée d’Alterac\n📍 Rivage bouillonnant")
                    .WithColor(Color.Red)
                    .WithUrl("https://www.wowhead.com/bg")
                    .Build();
                await ReplyAsync(embed: embed);
            }

            [Command("dpsmeta")]
            public async Task DpsMetaAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🔥 Meilleurs DPS du patch")
                    .WithDescription("Selon WarcraftLogs et Icy Veins :")
                    .AddField("1️⃣ Démoniste Destruction", "Top dégâts en multi-cibles.")
                    .AddField("2️⃣ Mage Givre", "Très stable et fort en burst.")
                    .AddField("3️⃣ Chasseur Précision", "Excellente mobilité.")
                    .WithUrl("https://www.icy-veins.com/wow/dps-rankings")
                    .WithColor(Color.DarkRed)
                    .Build();
                await ReplyAsync(embed: embed);
            }

            [Command("faq")]
            public async Task FaqAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("❓ Questions fréquentes")
                    .AddField("Comment rejoindre une guilde ?", "Tape `/guilde` ou demande en /2 Commerce.")
                    .AddField("Comment reset une instance ?", "Sors du donjon puis clic droit sur ton portrait > Réinitialiser.")
                    .AddField("Où trouver un entraîneur ?", "Les capitales en ont toujours un par métier.")
                    .WithColor(Color.LightGrey)
                    .Build();
                await ReplyAsync(embed: embed);
            }
        }

        private void SaveScores()
        {
            var json = JsonSerializer.Serialize(_userPoints, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ScoreFile, json);
        }

        public static void AddPoints(ulong userId, int amount)
        {
            _userPoints[userId] = _userPoints.GetValueOrDefault(userId) + amount;
            File.WriteAllText(ScoreFile, JsonSerializer.Serialize(_userPoints, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static int GetPoints(ulong userId) => _userPoints.GetValueOrDefault(userId);

        public static List<(ulong Id, int Points)> GetTopUsers(int count = 5)
        {
            var list = new List<(ulong Id, int Points)>();
            foreach (var pair in _userPoints)
                list.Add((pair.Key, pair.Value));

            list.Sort((a, b) => b.Points.CompareTo(a.Points));
            return list.GetRange(0, Math.Min(count, list.Count));
        }
    }

    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        [Command("info")]
        public async Task InfoAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("World of Warcraft")
                .WithDescription("🧙 MMORPG légendaire développé par Blizzard.")
                .WithColor(Color.DarkBlue)
                .AddField("Factions", "Alliance & Horde")
                .AddField("Univers", "Azeroth, Draenor, Ombreterre...")
                .Build();

            await ReplyAsync(embed: embed);
        }

        public class HelpInteractionModule : InteractionModuleBase<SocketInteractionContext>
        {
            [SlashCommand("help", "Affiche toutes les catégories de commandes du bot.")]
            public async Task HelpCommand()
            {
                var builder = new ComponentBuilder()
                    .WithButton("🎮 Quiz & Score", "btn_quiz")
                    .WithButton("📘 Lore", "btn_lore")
                    .WithButton("🧠 Builds", "btn_builds")
                    .WithButton("🏰 Raids", "btn_raids")
                    .WithButton("🎥 Vidéos", "btn_videos");

                var embed = new EmbedBuilder()
                    .WithTitle("📜 WoWGameInfo – Menu d’aide interactif")
                    .WithDescription("Clique sur une catégorie ci-dessous pour voir les commandes correspondantes.")
                    .WithColor(Color.DarkOrange)
                    .Build();

                await RespondAsync(embed: embed, components: builder.Build());
            }

            [ComponentInteraction("btn_quiz")]
            public async Task ShowQuizSection()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🎮 Quiz & Score")
                    .WithDescription("`/quiz` – Lancer un quiz\n`/reponse` – Répondre au quiz\n`/points`, `/classement`")
                    .WithColor(Color.Blue)
                    .Build();

                await RespondAsync(embed: embed, ephemeral: true);
            }

            [ComponentInteraction("btn_lore")]
            public async Task ShowLoreSection()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("📘 Lore & Univers")
                    .WithDescription("`/info`, `/classes`, `/races`, `/boss`, etc.")
                    .WithColor(Color.DarkBlue)
                    .Build();

                await RespondAsync(embed: embed, ephemeral: true);
            }

            [ComponentInteraction("btn_builds")]
            public async Task ShowBuildsSection()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🧠 Builds & Talents")
                    .WithDescription("`/build <classe>`, `/talents <classe>`, `/rotation <classe>`")
                    .WithColor(Color.Purple)
                    .Build();

                await RespondAsync(embed: embed, ephemeral: true);
            }

            [ComponentInteraction("btn_raids")]
            public async Task ShowRaidsSection()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🏰 Donjons & Raids")
                    .WithDescription("`/donjons`, `/raid <extension>`")
                    .WithColor(Color.DarkRed)
                    .Build();

                await RespondAsync(embed: embed, ephemeral: true);
            }

            [ComponentInteraction("btn_videos")]
            public async Task ShowVideosSection()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🎥 Vidéos WoW")
                    .WithDescription("`/videointro`, `/videoraid`, `/videobuild`, `/videoaddon`, `/videoactualite`")
                    .WithColor(Color.Teal)
                    .Build();

                await RespondAsync(embed: embed, ephemeral: true);
            }
        }


    }
    public class VideoModule : ModuleBase<SocketCommandContext>
    {
        private Embed BuildEmbed(string titre, string description, string url, Color couleur, string footer = null)
        {
            var embed = new EmbedBuilder()
                .WithTitle(titre)
                .WithDescription(description + $"\n\n🔗 [Voir la vidéo]({url})")
                .WithColor(couleur)
                .WithUrl(url);

            if (!string.IsNullOrWhiteSpace(footer))
                embed.WithFooter(footer);

            return embed.Build();
        }

        [Command("videointro")]
        public async Task VideoIntroAsync()
        {
            var embed = BuildEmbed(
                "🎬 Introduction à World of Warcraft",
                "Le trailer cinématique légendaire de World of Warcraft (Classic).",
                "https://www.youtube.com/watch?v=eYNCCu0y-Is",
                Color.DarkBlue,
                "Blizzard Entertainment – Trailer officiel"
            );

            await ReplyAsync(embed: embed);
        }

        [Command("videoraid")]
        public async Task VideoRaidAsync()
        {
            var embed = BuildEmbed(
                "📺 Guide vidéo : Ulduar (Wrath of the Lich King)",
                "Découvrez les mécaniques du raid Ulduar, l’un des plus appréciés de WoW.",
                "https://www.youtube.com/watch?v=GRqXsmgFaaI",
                Color.Gold,
                "Ulduar – Guide par un vétéran"
            );

            await ReplyAsync(embed: embed);
        }

        [Command("videobuild")]
        public async Task VideoBuildAsync([Remainder] string classe)
        {
            var recherche = $"wow {classe} build pve";
            var url = $"https://www.youtube.com/results?search_query={Uri.EscapeDataString(recherche)}";

            var embed = BuildEmbed(
                $"🔧 Builds vidéos pour {classe}",
                $"Voici une recherche YouTube pour trouver des guides de build PvE pour **{classe}**.",
                url,
                Color.Purple,
                "Résultats YouTube (non filtrés)"
            );

            await ReplyAsync(embed: embed);
        }

        [Command("videoaddon")]
        public async Task VideoAddonAsync()
        {
            var embed = BuildEmbed(
                "📦 Addons indispensables pour WoW",
                "Une sélection des meilleurs addons pour améliorer ton interface et ton gameplay.",
                "https://www.youtube.com/watch?v=7W4v6Z5jHd0",
                Color.Teal,
                "UI, quality of life, et outils de combat"
            );

            await ReplyAsync(embed: embed);
        }

        [Command("videoactualite")]
        public async Task VideoActuAsync()
        {
            var embed = BuildEmbed(
                "📰 Actualités WoW",
                "La chaîne officielle Blizzard propose les dernières bandes-annonces, annonces et mises à jour du jeu.",
                "https://www.youtube.com/@BlizzardEnt",
                Color.Orange,
                "Blizzard Entertainment – YouTube"
            );

            await ReplyAsync(embed: embed);
        }
    }

    public class QuizModule : ModuleBase<SocketCommandContext>
    {
        private static readonly Dictionary<ulong, string> _pendingAnswers = new();

        private readonly List<(string Question, string Answer)> _quizzes = new()
        {
            ("Quel est le nom du chef de la Horde ?", "Thrall"),
            ("Quel est le continent de départ des elfes de la nuit ?", "Kalimdor"),
            ("Qui est l'ancien roi déchu devenu le roi-liche ?", "Arthas")
        };

        [Command("quiz")]
        public async Task QuizAsync()
        {
            var quiz = _quizzes[new Random().Next(_quizzes.Count)];
            _pendingAnswers[Context.User.Id] = quiz.Answer;

            await ReplyAsync($"❓ **Quiz** : {quiz.Question}\nRéponds avec `!reponse ta_réponse`");
        }

        [Command("reponse")]
        public async Task ReponseAsync([Remainder] string userAnswer)
        {
            if (!_pendingAnswers.TryGetValue(Context.User.Id, out var correctAnswer))
            {
                await ReplyAsync("❗ Utilise `!quiz` d'abord.");
                return;
            }

            if (userAnswer.Trim().Equals(correctAnswer, StringComparison.OrdinalIgnoreCase))
            {
                Program.AddPoints(Context.User.Id, 1);
                await ReplyAsync($"✅ Bonne réponse ! Tu gagnes 1 point. Total : {Program.GetPoints(Context.User.Id)}");
                _pendingAnswers.Remove(Context.User.Id);
            }
            else
            {
                await ReplyAsync("❌ Mauvaise réponse !");
            }
        }

        [Command("points")]
        public async Task PointsAsync() =>
            await ReplyAsync($"🏆 {Context.User.Username}, tu as {Program.GetPoints(Context.User.Id)} point(s).");

        [Command("classement")]
        public async Task ClassementAsync()
        {
            var top = Program.GetTopUsers(5);
            var embed = new EmbedBuilder()
                .WithTitle("🏆 Classement des joueurs")
                .WithColor(Color.Gold);

            int rank = 1;
            foreach (var (id, points) in top)
            {
                var user = Context.Client.GetUser(id);
                string name = user?.Username ?? $"Inconnu ({id})";
                embed.AddField($"#{rank++} — {name}", $"{points} point(s)", inline: false);
            }

            await ReplyAsync(embed: embed.Build());
        }
    }

    public class WowLoreModule : ModuleBase<SocketCommandContext>
    {
        [Command("classes")]
        public async Task ClassesAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🧙 Classes jouables")
                .WithDescription("Guerrier, Mage, Voleur, Paladin, Druide, Démoniste, etc.")
                .WithUrl("https://www.wowhead.com/classes")
                .WithColor(Color.Blue)
                .Build();
            await ReplyAsync(embed: embed);
        }

        [Command("races")]
        public async Task RacesAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🧬 Races jouables")
                .WithDescription("Humain, Orc, Elfe de la nuit, Troll, Nain, etc.")
                .WithUrl("https://www.wowhead.com/races")
                .WithColor(Color.Green)
                .Build();
            await ReplyAsync(embed: embed);
        }

        [Command("factions")]
        public async Task FactionsAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("⚔️ Factions")
                .WithDescription("Alliance vs Horde : deux philosophies, deux styles.")
                .WithUrl("https://wowpedia.fandom.com/wiki/Faction")
                .WithColor(Color.Red)
                .Build();
            await ReplyAsync(embed: embed);
        }

        [Command("extensions")]
        public async Task ExtensionsAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("📦 Extensions de WoW")
                .WithDescription("Toutes les extensions depuis Classic jusqu'à Dragonflight.")
                .WithUrl("https://www.wowhead.com/expansions")
                .WithColor(Color.Purple)
                .Build();
            await ReplyAsync(embed: embed);
        }

        [Command("boss")]
        public async Task BossAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("👹 Boss emblématiques")
                .WithDescription("Arthas, Illidan, Ragnaros, etc.")
                .WithUrl("https://www.wowhead.com/npcs")
                .WithColor(Color.DarkRed)
                .Build();
            await ReplyAsync(embed: embed);
        }

        [Command("zones")]
        public async Task ZonesAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🌍 Zones de jeu")
                .WithDescription("Explore Azeroth, Norfendre, Outreterre, etc.")
                .WithUrl("https://www.wowhead.com/zones")
                .WithColor(Color.Teal)
                .Build();
            await ReplyAsync(embed: embed);
        }

        [Command("metiers")]
        public async Task MetiersAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🛠️ Métiers")
                .WithDescription("Forge, Alchimie, Enchantement, etc.")
                .WithUrl("https://www.wowhead.com/professions")
                .WithColor(Color.Gold)
                .Build();
            await ReplyAsync(embed: embed);
        }

        [Command("role")]
        public async Task RoleAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🎭 Rôles en combat")
                .WithDescription("Tank, Soigneur, DPS – chacun est essentiel !")
                .WithUrl("https://wowpedia.fandom.com/wiki/Role")
                .WithColor(Color.DarkMagenta)
                .Build();
            await ReplyAsync(embed: embed);
        }
        [Command("heureeu")]
        public async Task HeureEuAsync()
        {
            var heureEu = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));
            await ReplyAsync($"🕒 Heure serveur EU (CET) : `{heureEu:HH:mm:ss}`");
        }
        [Command("reset")]
        public async Task ResetAsync()
        {
            var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time"));
            var prochainReset = now.Date.AddDays((3 - (int)now.DayOfWeek + 7) % 7).AddHours(9); // Mercredi 9h CET

            if (now > prochainReset) prochainReset = prochainReset.AddDays(7);

            var reste = prochainReset - now;
            await ReplyAsync($"⏳ Prochain reset hebdomadaire : **{prochainReset:dddd HH:mm}** (dans {reste.Days}j {reste.Hours}h {reste.Minutes}min)");
        }

        [Command("capitales")]
        public async Task CapitalesAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🏰 Capitales majeures")
                .WithDescription("Orgrimmar, Hurlevent, Lune-d’Argent, Darnassus...")
                .WithUrl("https://www.wowhead.com/zone=1519") // Hurlevent
                .WithColor(Color.LightGrey)
                .Build();
            await ReplyAsync(embed: embed);
        }
        [Command("newsrss")]
        public async Task NewsRssAsync()
        {
            var url = "https://www.wowhead.com/blue-tracker?rss";
            var client = new HttpClient();
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows)");

            using var stream = await client.GetStreamAsync(url);
            var xml = XDocument.Load(stream);
            var items = xml.Descendants("item").Take(5)
                .Select(x => new {
                    Title = (string)x.Element("title"),
                    Link = (string)x.Element("link")
                }).ToList();

            var embed = new EmbedBuilder()
                .WithTitle("📰 Blue Tracker – WoW News")
                .WithColor(Color.DarkBlue);

            foreach (var it in items)
                embed.AddField(it.Title, $"[Lire]({it.Link})");

            await ReplyAsync(embed: embed.Build());
        }


        [Command("serveurs")]
        public async Task ServeursAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🖥️ Types de serveurs")
                .WithDescription("PVE, PVP, RP, RPPVP — à chacun son ambiance !")
                .WithUrl("https://eu.forums.blizzard.com/fr/wow/t/guide-types-de-royaumes/")
                .WithColor(Color.Orange)
                .Build();
            await ReplyAsync(embed: embed);
        }
    }
    public class NewsModule : ModuleBase<SocketCommandContext>
    {
        [Command("news")]
        public async Task NewsAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🌟 Dernières Actualités WoW")
                .WithColor(Color.DarkBlue)
                .WithDescription("Mets-toi à jour avec les patchs, récits officiels et actualités du jour.")
                .AddField("🔥 Hotfixes – 10 juin 2025", "[Détails des changements de classes et sorts](https://news.blizzard.com/en-us/article/24201420/hotfixes-june-10-2025)")
                .AddField("📖 Patch 11.1.7 – Legacy of Arathor", "[Preview & récompenses](https://gamerant.com/world-of-warcraft-patch-11-1-7-campaign-rewards-story-details/)")
                .AddField("✍️ Nouvelle Short Story : Faith & Flame", "[Lire sur Blizzard](https://news.blizzard.com/en-us/article/24209851/warcraft-short-story-faith-flame)")
                .WithFooter($"Actualisé : {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC")
                .Build();

            await ReplyAsync(embed: embed);
        }
    }

}
