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
using static System.Net.WebRequestMethods;
using System.Net.Http;
using SummaryAttribute = Discord.Interactions.SummaryAttribute;

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

            string token = "Token"; // ⚠️ Ne jamais laisser en clair

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
            private static readonly Dictionary<string, string> classMap = new()
            {
                ["demoniste"] = "warlock",
                ["chevalier de la mort"] = "death-knight",
                ["mage"] = "mage",
                ["voleur"] = "rogue",
                ["paladin"] = "paladin",
                ["pretre"] = "priest",
                ["guerrier"] = "warrior",
                ["chasseur"] = "hunter",
                ["chaman"] = "shaman",
                ["druide"] = "druid",
                ["moine"] = "monk",
                ["chasseur de demons"] = "demon-hunter"
            };



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
            [Command("meta")]
            public async Task MetaAsync([Remainder] string classe)
            {
                string url = $"https://www.icy-veins.com/wow/{classe.ToLower()}-dps-rankings-tier-list";
                var embed = new EmbedBuilder()
                    .WithTitle($"📈 Tier List / Meta pour {classe}")
                    .WithDescription("Selon Icy Veins, voici les performances actuelles de la classe.")
                    .WithUrl(url)
                    .WithColor(Color.DarkMagenta)
                    .Build();
                await ReplyAsync(embed: embed);
            }

            [Command("evenement")]
            public async Task EvenementAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🎉 Événements WoW en cours")
                    .AddField("🔥 Fête du Feu", "Du 21 juin au 5 juillet")
                    .AddField("🎪 Foire de Sombrelune", "Du 7 au 13 de chaque mois")
                    .AddField("📦 Bonus Donjons", "Cette semaine : +25% récompenses de fin de donjon")
                    .WithFooter("Pour plus d'infos : wowhead.com/events")
                    .WithColor(Color.Orange)
                    .Build();
                await ReplyAsync(embed: embed);
            }

            [Command("nomrp")]
            public async Task NomRpAsync([Remainder] string race)
            {
                string[] noms = race.ToLower() switch
                {
                    "orc" => new[] { "Gor'thaz", "Mokgrol", "Throgar" },
                    "humain" => new[] { "Ellyra", "Darian", "Cedric" },
                    "elfe" => new[] { "Sylvaria", "Lor'thael", "Thalindra" },
                    _ => new[] { "Aeryn", "Kael", "Zun" }
                };

                string nom = noms[new Random().Next(noms.Length)];
                await ReplyAsync($"🎭 Nom RP suggéré pour {race} : **{nom}**");
            }

            [Command("blaguewow")]
            public async Task BlagueAsync()
            {
                string[] blagues = {
            "Pourquoi les paladins n'ont pas peur des fantômes ? Parce qu'ils ont *lumière sacrée*!",
            "Quel est le comble pour un démoniste ? De ne pas avoir d'amis infernaux.",
            "Les taurens n'ont pas de monture. Ils sont leur propre monture."
        };
                await ReplyAsync($"😂 {blagues[new Random().Next(blagues.Length)]}");
            }

            [Command("routefarm")]
            public async Task RouteFarmAsync([Remainder] string ressource)
            {
                var embed = new EmbedBuilder()
                    .WithTitle($"🌿 Route de farm recommandée pour : {ressource}")
                    .WithDescription(
                        $"Voici quelques zones efficaces pour récolter **{ressource}** :\n" +
                        "• **Hautes-terres d'Arathi**\n" +
                        "• **Vallée de Strangleronce**\n" +
                        "• **Zuldazar**\n\n" +
                        "🔗 [Voir d'autres guides sur wow-professions.com](https://www.wow-professions.com)")
                    .WithColor(Color.Green)
                    .Build();

                await ReplyAsync(embed: embed);
            }


            [Command("astuceclasse")]
            public async Task AstuceClasseAsync([Remainder] string classe)
            {
                string[] astuces = classe.ToLower() switch
                {
                    "mage" => new[] { "Utilise Nova de givre avant de blink pour survivre.", "Économise ton burst pour les packs de trashs." },
                    "druide" => new[] { "Utilise les soins HOT avant les gros dégâts.", "Pense à Cyclone en PvP !" },
                    _ => new[] { "Utilise toujours ton cooldown défensif avant les gros dégâts." }
                };
                await ReplyAsync($"💡 Astuce pour {classe} : {astuces[new Random().Next(astuces.Length)]}");
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
            if (System.IO.File.Exists(ScoreFile))
            {
                var json = System.IO.File.ReadAllText(ScoreFile);
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
            System.IO.File.WriteAllText(ScoreFile, json);
        }

        public static void AddPoints(ulong userId, int amount)
        {
            _userPoints[userId] = _userPoints.GetValueOrDefault(userId) + amount;
            System.IO.File.WriteAllText(ScoreFile, JsonSerializer.Serialize(_userPoints, new JsonSerializerOptions { WriteIndented = true }));
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
            private Embed embed;
            private object http;

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

            [SlashCommand("menu", "Affiche le menu général WoW avec ressources et actualités.")]
            public async Task MenuCommand()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("📚 WoWGameInfo – Menu Principal")
                    .WithDescription("Explore les ressources du jeu, builds, vidéos, actualités, etc.")
                    .WithColor(Color.Blue)
                    .WithUrl("https://worldofwarcraft.blizzard.com")
                    .AddField("🌐 Sites utiles",
                        "[Wowhead](https://www.wowhead.com) | " +
                        "[Icy Veins](https://www.icy-veins.com/wow/) | " +
                        "[WoW Professions](https://www.wow-professions.com) | " +
                        "[MMO-Champion](https://www.mmo-champion.com)")
                    .AddField("📰 Dernières News",
                        "👉 Tape `/newsrss` pour les dernières infos officielles\n👉 Tape `/news` pour les actus du jour")
                    .WithFooter("Utilise les boutons ci-dessous pour naviguer par thème.")
                    .Build();

                var buttons = new ComponentBuilder()
                    .WithButton("🎥 Vidéos", "btn_videos", ButtonStyle.Primary)
                    .WithButton("⚔️ Builds", "btn_builds", ButtonStyle.Secondary)
                    .WithButton("📖 Lore", "btn_lore", ButtonStyle.Success)
                    .WithButton("📊 DPS Meta", "btn_dpsmeta", ButtonStyle.Danger);

                await RespondAsync(embed: embed, components: buttons.Build());
            }

            [SlashCommand("extensions", "Liste toutes les extensions WoW avec leurs liens.")]
            public async Task ExtensionsAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("📦 Extensions majeures de World of Warcraft")
                    .WithDescription("Voici la liste complète des extensions :")
                    .WithColor(Color.DarkGreen)
                    .AddField("Classic", "[wowwiki](https://wowwiki-archive.fandom.com/wiki/World_of_Warcraft:_Classic)")
                    .AddField("Burning Crusade", "[wowhead](https://www.wowhead.com/tbc/)")
                    .AddField("Wrath of the Lich King", "[wiki](https://wowpedia.fandom.com/wiki/Wrath_of_the_Lich_King)")
                    .AddField("Mists of Pandaria", "[info](https://wowpedia.fandom.com/wiki/Mists_of_Pandaria)")
                    .AddField("Dragonflight", "[site](https://worldofwarcraft.blizzard.com/en-us/dragonflight)")
                    .Build();
                await RespondAsync(embed: embed);
            }
            [SlashCommand("youtube", "Chaînes YouTube utiles pour les joueurs WoW.")]
            public async Task YouTubeAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("📺 Chaînes YouTube pour t'informer sur WoW")
                    .WithColor(Color.Purple)
                    .AddField("Blizzard (Officiel)", "https://www.youtube.com/@BlizzardEnt")
                    .AddField("Icy Veins", "https://www.youtube.com/@icyveins")
                    .AddField("Nixxiom (Lore & Humor)", "https://www.youtube.com/@Nixxiom")
                    .AddField("HazelNuttyGames (Guides)", "https://www.youtube.com/@HazelNuttygames")
                    .Build();
                await RespondAsync(embed: embed);
            }
            [SlashCommand("lore-encyclopedia", "Explore un sujet précis du lore de WoW.")]
            public async Task LoreEncyclopediaAsync([Discord.Interactions.Summary(description: "Ex: Illidan, Légion, Pandarie")] string sujet)
            {
                string url = $"https://wowpedia.fandom.com/wiki/{Uri.EscapeDataString(sujet)}";
                var embed = new EmbedBuilder()
                    .WithTitle($"📘 Lore : {sujet}")
                    .WithDescription($"Découvre l'histoire de **{sujet}** sur Wowpedia.")
                    .WithUrl(url)
                    .WithColor(Color.DarkPurple)
                    .WithFooter("Source : Wowpedia")
                    .Build();

                await RespondAsync(embed: embed);
            }
            [SlashCommand("class-guide", "Obtiens un guide PvE basé sur ta classe et spécialisation.")]
            public async Task ClassGuideAsync(
    [Summary(description: "Classe ex: mage")] string classe,
    [Summary(description: "Spécialisation ex: feu, sacré")] string specialisation)
            {
                string url = $"https://www.icy-veins.com/wow/{classe.ToLower()}-{specialisation.ToLower()}-pve-guide";
                var embed = new EmbedBuilder()
                    .WithTitle($"⚔️ Guide {classe} – {specialisation}")
                    .WithDescription("Guide détaillé de rotation, talents, stats et équipement.")
                    .WithUrl(url)
                    .WithColor(Color.Blue)
                    .Build();

                await RespondAsync(embed: embed);
            }

            [SlashCommand("lore-sites", "Sites à consulter pour le lore de WoW.")]
            public async Task LoreSitesAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("📘 Sites pour explorer l'histoire de WoW")
                    .WithColor(Color.Blue)
                    .AddField("Wowpedia", "https://wowpedia.fandom.com")
                    .AddField("Wowhead Lore", "https://www.wowhead.com/lore-guides")
                    .AddField("Chronicles Fanpage", "https://warcraftchronicle.com")
                    .Build();
                await RespondAsync(embed: embed);
            }
            [SlashCommand("dps-meta", "Classement actuel des meilleurs DPS.")]
            public async Task DpsMetaAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🔥 DPS META – Patch actuel")
                    .WithColor(Color.Orange)
                    .AddField("1️⃣ Démoniste Destruction", "Excellent en multi-cibles.")
                    .AddField("2️⃣ Mage Givre", "Fort burst + contrôles.")
                    .AddField("3️⃣ Chasseur Précision", "Très mobile.")
                    .WithFooter("Source : icy-veins.com / warcraftlogs.com")
                    .Build();
                await RespondAsync(embed: embed);
            }

            [SlashCommand("build", "Affiche le guide PvE pour une classe.")]
            public async Task BuildAsync([Discord.Interactions.Summary(description: "Exemple : mage, paladin, etc.")] string classe)
            {
                string url = $"https://www.icy-veins.com/wow/{classe.ToLower()}-pve-guide";
                var embed = new EmbedBuilder()
                    .WithTitle($"⚔️ Build recommandé : {classe}")
                    .WithDescription("Guide PvE complet par Icy Veins.")
                    .WithUrl(url)
                    .WithColor(Color.DarkBlue)
                    .Build();

                await RespondAsync(embed: embed);
            }
            [SlashCommand("zones", "Liste des grandes zones de World of Warcraft.")]
            public async Task ZonesAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🌍 Zones emblématiques")
                    .WithColor(Color.Green)
                    .AddField("Azeroth", "Forêt d’Elwynn, Durotar, Strangleronce…")
                    .AddField("Norfendre", "Toundra Boréenne, Couronne de glace")
                    .AddField("Outreterre", "Péninsule des Flammes infernales, Nagrand")
                    .WithUrl("https://www.wowhead.com/zones")
                    .Build();
                await RespondAsync(embed: embed, ephemeral: true);
            }

            [SlashCommand("boss", "Boss mythiques de toutes les extensions.")]
            public async Task BossAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("👹 Boss emblématiques")
                    .WithDescription("Retrouve les boss légendaires de WoW.")
                    .AddField("🧊 Le Roi-Liche (Arthas)", "[Citadelle de la Couronne de glace](https://www.wowhead.com/zone=4812)")
                    .AddField("🔥 Ragnaros", "[Cœur du Magma](https://www.wowhead.com/zone=2717)")
                    .AddField("👁️ C’Thun", "[Ahn'Qiraj](https://www.wowhead.com/zone=3429)")
                    .WithColor(Color.Red)
                    .Build();
                await RespondAsync(embed: embed, ephemeral: true);
            }
            [SlashCommand("zone-info", "Infos d'une zone WoW : description, image, lien")]
            public async Task ZoneInfoAsync([Discord.Interactions.Summary(description: "Ex: durotar, elwynn, zuldazar")] string zone)

            {
                var zoneName = zone.ToLower().Trim();

                var zones = new Dictionary<string, (string desc, string lien, string image)>
                {
                    ["durotar"] = (
                        "Zone de départ des orcs, aride et rougeoyante, située à l'est de Kalimdor.",
                        "https://www.wowhead.com/zone=14/durotar",
                        "https://static.wikia.nocookie.net/wowwiki/images/4/41/Durotar.jpg"
                    )
                    // Ajoute d'autres zones ici
                };

                if (!zones.TryGetValue(zoneName, out var data))
                {
                    await RespondAsync($"❌ Zone inconnue : `{zone}`. Essaie par exemple `durotar`, `elwynn`, `zuldazar`.");
                    return;
                }

                var embed = new EmbedBuilder()
                    .WithTitle($"📍 Zone : {zone}")
                    .WithDescription(data.desc)
                    .WithUrl(data.lien)
                    .WithImageUrl(data.image)
                    .WithColor(Color.DarkGreen)
                    .Build();

                await RespondAsync(embed: embed);
            }

            [SlashCommand("professions", "Liste des métiers principaux.")]
            public async Task ProfessionsAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🛠️ Métiers de WoW")
                    .WithColor(Color.Orange)
                    .WithDescription("Forge, Alchimie, Dépeçage, Couture, Enchantement...")
                    .WithUrl("https://www.wowhead.com/professions")
                    .Build();
                await RespondAsync(embed: embed, ephemeral: true);
            }
            [SlashCommand("zone-activity", "Affiche l'activité et les infos d'une zone de World of Warcraft.")]
            public async Task ZoneActivityAsync(
    [Discord.Interactions.Summary(description: "Nom de la zone (ex: Durotar, Zuldazar, Elwynn)")] string zone)
            {
                var zoneName = zone.ToLower().Trim();

                var zones = new Dictionary<string, (string desc, string lien, string image, string activite)>
                {
                    ["durotar"] = (
                        "Zone aride de Kalimdor, terre natale des orcs.",
                        "https://www.wowhead.com/zone=14/durotar",
                        "https://static.wikia.nocookie.net/wowwiki/images/4/41/Durotar.jpg",
                        "🌩️ Orages fréquents\n🛡️ Activité PVP : moyenne\n🔄 Quêtes journalières disponibles"
                    ),
                    ["zuldazar"] = (
                        "Ancienne capitale troll Zandalari. Jungle dense et menaçante.",
                        "https://www.wowhead.com/zone=862/zuldazar",
                        "https://static.wikia.nocookie.net/wowwiki/images/3/39/Zuldazar.jpg",
                        "🎯 Invasions mineures\n⚔️ Donjons proches : Atal'Dazar\n📦 Ressources : herbes tropicales"
                    ),
                    ["elwynn"] = (
                        "Plaine verdoyante des humains autour de Hurlevent.",
                        "https://www.wowhead.com/zone=12/elwynn-forest",
                        "https://static.wikia.nocookie.net/wowwiki/images/f/f0/Elwynn_Forest.jpg",
                        "🐺 Faible activité ennemie\n🛍️ Zones de farm bas niveau\n📘 Lieu de départ RP classique"
                    ),
                    ["nagrand"] = (
                        "Une savane épique de l'Outreterre, avec des clairières flottantes.",
                        "https://www.wowhead.com/zone=3518/nagrand",
                        "https://static.wikia.nocookie.net/wowwiki/images/2/2c/Nagrand.jpg",
                        "🦐 Pêche exotiques\n📦 Ressources : minerais rares\n🏇 Événements arènes"
                    ),
                    ["tirisfal"] = (
                        "Terres sombres du royaume déchu de Lordaeron.",
                        "https://www.wowhead.com/zone=85/tirisfal-glades",
                        "https://static.wikia.nocookie.net/wowwiki/images/2/23/Tirisfal_Glades.jpg",
                        "☠️ Zone RP sombre\n👻 Présence de morts-vivants\n🔮 Quêtes de magie noire"
                    ),
                    ["suramar"] = (
                        "Cité elfique cachée et protégée par des arcanes anciennes.",
                        "https://www.wowhead.com/zone=7637/suramar",
                        "https://static.wikia.nocookie.net/wowwiki/images/d/d6/Suramar.jpg",
                        "✨ Quêtes de mana\n🏛️ Architecture elfique\n🕵️ Infiltration et camouflages"
                    ),
                    ["stormpeaks"] = (
                        "Montagnes enneigées du Norfendre, riches en légendes titanesques.",
                        "https://www.wowhead.com/zone=67/the-storm-peaks",
                        "https://static.wikia.nocookie.net/wowwiki/images/6/6e/Storm_Peaks.jpg",
                        "🧭 Hauts lieux d'exploration\n⚒️ Excavations titanesques\n🐉 Vols draconiques"
                    ),
                    ["valsharah"] = (
                        "Forêt mystique, sanctuaire des druides et de la nature.",
                        "https://www.wowhead.com/zone=7558/valsharah",
                        "https://static.wikia.nocookie.net/wowwiki/images/3/3a/Valsharah.jpg",
                        "🌳 Présence d’Ysera\n🌀 Portails vers le Rêve d’émeraude\n🦉 Faune magique"
                    )
                };

                if (!zones.TryGetValue(zoneName, out var data))
                {
                    var suggestions = string.Join(", ", zones.Keys.Select(z => $"`{z}`"));
                    await RespondAsync($"❌ Zone inconnue : `{zone}`. Suggestions : {suggestions}");
                    return;
                }

                var embed = new EmbedBuilder()
                    .WithTitle($"📍 Activité en zone : {char.ToUpper(zone[0]) + zone[1..]}")
                    .WithDescription(data.desc)
                    .AddField("🔎 Activité actuelle", data.activite)
                    .WithUrl(data.lien)
                    .WithImageUrl(data.image)
                    .WithColor(Color.DarkGreen)
                    .WithFooter("Infos RP et lore contextuelles")
                    .Build();

                await RespondAsync(embed: embed);
            }

            [SlashCommand("factions", "Alliance, Horde et bien plus.")]
            public async Task FactionsAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("⚔️ Factions majeures")
                    .AddField("Alliance", "Humains, Nains, Elfes de la nuit…")
                    .AddField("Horde", "Orcs, Taurens, Trolls…")
                    .AddField("Neutres", "Pandarens, Cartel Gentepression…")
                    .WithUrl("https://wowpedia.fandom.com/wiki/Faction")
                    .WithColor(Color.Blue)
                    .Build();
                await RespondAsync(embed: embed, ephemeral: true);
            }

            [SlashCommand("mop-classic", "Infos et lien vers MoP Classic")]
            public async Task MopClassicAsync()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("🐼 Mists of Pandaria Classic")
                    .WithDescription("La version revisitée de MoP est de retour !")
                    .WithUrl("https://worldofwarcraft.blizzard.com/en-us/game/mists-of-pandaria-classic")
                    .WithColor(Color.Teal)
                    .AddField("Nouvelles fonctionnalités", "Scénarios, Moine, Pandarie, Donjons épiques")
                    .Build();

                await RespondAsync(embed: embed, ephemeral: true);
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
            [ComponentInteraction("btn_dpsmeta")]
            public async Task HandleDpsMeta()
            {
                var embed = new EmbedBuilder()
                    .WithTitle("📊 DPS META (actuel)")
                    .WithDescription("Classement actuel selon les sources :\n- [Icy Veins DPS Rankings](https://www.icy-veins.com/wow/dps-rankings)\n- [Warcraft Logs](https://www.warcraftlogs.com/)")
                    .WithColor(Color.Orange)
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
        [Command("build mop")]
        public async Task BuildMopAsync([Remainder] string classe)
        {
            string url = $"https://www.icy-veins.com/wow/{classe.ToLower()}-pve-guide";
            var embed = new EmbedBuilder()
                .WithTitle($"⚔️ Build MoP – {classe}")
                .WithDescription("Guide PvE complet pour MoP Classic (non officiel).")
                .WithUrl(url)
                .WithColor(Color.Orange)
                .Build();

            await ReplyAsync(embed: embed);
        }

        [Command("raid mop")]
        public async Task RaidMopAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🏯 Raids emblématiques – MoP Classic")
                .WithColor(Color.DarkRed)
                .AddField("Cœur de la peur", "[Wowhead](https://www.wowhead.com/zone=6297)")
                .AddField("Terrasse Printanière", "[Wowhead](https://www.wowhead.com/zone=6622)")
                .AddField("Siège d’Orgrimmar", "[Wowhead](https://www.wowhead.com/zone=6738)")
                .AddField("Trône du Tonnerre", "[Wowhead](https://www.wowhead.com/zone=6623)");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("zones mop")]
        public async Task ZonesMopAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🌿 Zones majeures – Pandarie")
                .WithColor(Color.Green)
                .WithDescription("Explore les zones suivantes :\n- Vallée des Quatre vents\n- Sommet de Kun-Lai\n- Steppes de Tanglong\n- Vallée de l’Éternel printemps")
                .WithUrl("https://www.wowhead.com/zones/mop");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("lore mop")]
        public async Task LoreMopAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("📖 Lore – MoP")
                .WithColor(Color.Purple)
                .WithDescription("La Pandarie est un continent caché longtemps inconnu d'Azeroth, protégé par les Pandarens.\n\nL'invasion des Sha, les mogu, et le siège d'Orgrimmar ont marqué cette ère.")
                .WithUrl("https://wowpedia.fandom.com/wiki/Mists_of_Pandaria");

            await ReplyAsync(embed: embed.Build());
        }

        [Command("metier mop")]
        public async Task MetierMopAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🛠️ Métiers recommandés – MoP")
                .WithColor(Color.Blue)
                .WithDescription("🔹 Cuisine Pandarène (Banquets, nourriture OP)\n🔹 Calligraphie pour parchemins et talents\n🔹 Forge & Enchantement pour optimisations d’équipement")
                .WithUrl("https://www.wow-professions.com/mop-profession-leveling");

            await ReplyAsync(embed: embed.Build());
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
                .Select(x => new
                {
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

        [Command("recherche")]
        public async Task RechercheAsync([Remainder] string terme)
        {
            var url = $"https://www.wowhead.com/search?q={Uri.EscapeDataString(terme)}";

            var embed = new EmbedBuilder()
                .WithTitle($"🔍 Résultat de recherche pour : {terme}")
                .WithDescription("Clique ci-dessous pour consulter les résultats sur Wowhead.")
                .WithUrl(url)
                .WithColor(Color.Gold)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("meteo")]
        public async Task MeteoAsync([Remainder] string zone)
        {
            var embed = new EmbedBuilder()
                .WithTitle($"☁️ Météo dans {zone}")
                .WithDescription($"Dans **{zone}**, les conditions sont souvent :\n🌧️ Pluie légère\n🌫️ Brouillard magique\n🌞 Éclaircies solaires")
                .WithColor(Color.LightGrey)
                .WithFooter("Estimation roleplay, pas en temps réel")
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("craft")]
        public async Task CraftAsync([Remainder] string objet)
        {
            var url = $"https://www.wowhead.com/search?q={Uri.EscapeDataString(objet)}";
            var embed = new EmbedBuilder()
                .WithTitle($"🛠️ Guide de craft : {objet}")
                .WithDescription("Consulte les composants et plans nécessaires.")
                .WithUrl(url)
                .WithColor(Color.Orange)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("sac")]
        public async Task SacAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🎒 Gestion d'inventaire")
                .WithDescription("💡 Astuces :\n- Trie régulièrement les sacs.\n- Utilise des sacs spécialisés (herboriste, mineur…)\n- Vends les objets gris automatiquement avec un addon comme Scrap.")
                .WithColor(Color.DarkOrange)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("reputations")]
        public async Task ReputationsAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("📜 Réputations importantes")
                .WithDescription("Voici quelques réputations clés à monter pour du stuff ou des recettes.")
                .AddField("👑 Kirin Tor", "[Voir](https://www.wowhead.com/faction=1090/kirin-tor)")
                .AddField("🐉 Accord d’Alexstrasza", "[Voir](https://www.wowhead.com/faction=2507/dragonscale-expedition)")
                .AddField("☯️ Pandashan", "[Voir](https://www.wowhead.com/faction=1271/shado-pan)")
                .AddField("⚔️ Main de l’Aube", "[Voir](https://www.wowhead.com/faction=529/the-argent-dawn)")
                .WithColor(Color.Teal)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("addons")]
        public async Task AddonsAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("📦 Addons WoW recommandés")
                .AddField("🔎 Details (DPS Meter)", "[Lien CurseForge](https://www.curseforge.com/wow/addons/details)")
                .AddField("📜 WeakAuras", "[Lien CurseForge](https://www.curseforge.com/wow/addons/weakauras-2)")
                .AddField("🧹 Leatrix Plus", "[Lien CurseForge](https://www.curseforge.com/wow/addons/leatrix-plus)")
                .AddField("📦 Bagnon (inventaire)", "[Lien CurseForge](https://www.curseforge.com/wow/addons/bagnon)")
                .WithColor(Color.Blue)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("pvpmeta")]
        public async Task PvpMetaAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("⚔️ Top classes PvP (3v3 Arena)")
                .AddField("1️⃣ Guerrier Armes", "Haute pression & burst")
                .AddField("2️⃣ Chaman Amélioration", "Utility + burst")
                .AddField("3️⃣ Démoniste Affliction", "Contrôle + DoTs")
                .WithUrl("https://www.skill-capped.com/")
                .WithColor(Color.Red)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("sitesutiles")]
        public async Task SitesUtilesAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🌍 Sites incontournables WoW")
                .AddField("🔎 Wowhead", "https://www.wowhead.com/")
                .AddField("📘 Icy Veins (Guides)", "https://www.icy-veins.com/wow/")
                .AddField("📊 WarcraftLogs", "https://www.warcraftlogs.com/")
                .AddField("🛠️ CurseForge", "https://www.curseforge.com/")
                .AddField("📡 MMO-Champion", "https://www.mmo-champion.com/")
                .WithColor(Color.Purple)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("objectifs")]
        public async Task ObjectifsAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🎯 Objectifs hebdo WoW")
                .WithDescription("Voici ce que tu peux viser chaque semaine :")
                .AddField("🗺️ Expéditions", "Fais au moins 4 quêtes de faction.")
                .AddField("🛡️ Donjons Mythiques", "Complète au moins un +10 pour la grande chambre.")
                .AddField("⚔️ PVP hebdo", "Gagne des matchs cotés pour des récompenses.")
                .AddField("🏆 Tour des Mages", "[Infos ici](https://www.icy-veins.com/wow/mage-tower-guide)")
                .WithColor(Color.Gold)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("megalore")]
        public async Task MegaLoreAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("📖 Méga Lore de World of Warcraft")
                .WithDescription("Voyage à travers toute l’histoire de World of Warcraft. Chaque extension, chaque conflit, chaque héros. 🌍")
                .WithColor(Color.DarkPurple)
                .WithFooter("Sources officielles : WoWpedia, Wowhead, Blizzard");

            embed
                .AddField("🧙 Classic (Vanilla)", "[Résumé complet](https://wowpedia.fandom.com/wiki/Timeline_(WoW)) – Le monde d’Azeroth renaît après les guerres. Premiers conflits entre races, guerre contre le Fléau et résurgence de la Légion.")
                .AddField("🔥 The Burning Crusade", "[Histoire](https://wowpedia.fandom.com/wiki/The_Burning_Crusade) | [Zones](https://www.wowhead.com/outland) – Illidan, les Naaru, l'Outreterre. Le portail noir est ouvert…")
                .AddField("☠️ Wrath of the Lich King", "[Lore](https://wowpedia.fandom.com/wiki/Wrath_of_the_Lich_King) | [Arthas](https://wowpedia.fandom.com/wiki/Arthas_Menethil) – La croisade vers Norfendre. Le roi-liche attend au sommet de la Citadelle.")
                .AddField("🌋 Cataclysm", "[Histoire](https://wowpedia.fandom.com/wiki/Cataclysm) | [Aile de Mort](https://wowpedia.fandom.com/wiki/Deathwing) – Azeroth est déchirée par les éléments. Nouvelles zones et races.")
                .AddField("🐼 Mists of Pandaria", "[Pandarie](https://wowpedia.fandom.com/wiki/Mists_of_Pandaria) | [Sha](https://wowpedia.fandom.com/wiki/Sha) – L’Empire Pandaren, ses secrets, et le retour de la guerre entre factions.")
                .AddField("⚙️ Warlords of Draenor", "[Draenor](https://wowpedia.fandom.com/wiki/Warlords_of_Draenor) | [Gul'dan](https://wowpedia.fandom.com/wiki/Gul%27dan) – Une timeline parallèle, la Horde de Fer, et les origines des orcs.")
                .AddField("💚 Legion", "[Lore](https://wowpedia.fandom.com/wiki/Legion_(expansion)) | [Illidan](https://wowpedia.fandom.com/wiki/Illidan_Stormrage) – L’assaut final de la Légion ardente. Artefacts, titans et rédemption.")
                .AddField("⚔️ Battle for Azeroth", "[Conflit global](https://wowpedia.fandom.com/wiki/Battle_for_Azeroth) | [Sylvanas](https://wowpedia.fandom.com/wiki/Sylvanas_Windrunner) – Azerite, anciens dieux, Teldrassil en feu.")
                .AddField("💀 Shadowlands", "[Au-delà](https://wowpedia.fandom.com/wiki/Shadowlands) | [Le Geôlier](https://wowpedia.fandom.com/wiki/The_Jailer) – La mort a une volonté, et Sylvanas brise le voile.")
                .AddField("🐉 Dragonflight", "[Îles aux dragons](https://wowpedia.fandom.com/wiki/Dragonflight) | [Aspects](https://wowpedia.fandom.com/wiki/Dragonflight_(faction)) – Les aspects reviennent, l’ancien monde s’éveille.")
                .AddField("🌸 MoP Classic", "[Annonce Blizzard](https://worldofwarcraft.blizzard.com/fr-fr/news/24031582) – Le retour de Pandarie en version Classic. Lore intact et nostalgie assurée !");

            await ReplyAsync(embed: embed.Build());
        }
        [Command("spotfarm")]
        public async Task SpotFarmAsync([Remainder] string ressource)
        {
            var url = $"https://www.wow-professions.com/farming/{Uri.EscapeDataString(ressource.ToLower())}";

            var embed = new EmbedBuilder()
                .WithTitle($"🌾 Zones de farm pour : {ressource}")
                .WithDescription("Voici une route de farm recommandée.")
                .WithUrl(url)
                .WithColor(Color.Green)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("wowfunfact")]
        public async Task WowFunFactAsync()
        {
            string[] facts =
            {
        "La danse du troll mâle est inspirée de MC Hammer.",
        "Il existe un PNJ du nom de Linken dans Un’Goro Crater – clin d’œil à Zelda.",
        "La lune de Draenor s'appelle Argus, qui deviendra une planète visitable plus tard.",
        "Thrall a été doublé par Chris Metzen, créateur de WoW."
    };

            await ReplyAsync($"🎉 **Fun Fact WoW :** {facts[new Random().Next(facts.Length)]}");
        }
        [Command("serveursactifs")]
        public async Task ServeursActifsAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🖥️ Statut des serveurs européens")
                .WithDescription("Consulte les statuts officiels des royaumes Blizzard :")
                .WithUrl("https://eu.battle.net/support/fr/article/76459")
                .WithColor(Color.Orange)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("metapvp")]
        public async Task MetaPvpAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("⚔️ Méta PvP actuelle")
                .WithDescription("Top classes en arène & champs de bataille (patch récent).")
                .AddField("🥇 Spé top", "Guerrier Armes, Chasseur Survie, Moine Marche-vent")
                .AddField("💡 Source", "[Wowhead PvP Meta](https://www.wowhead.com/guides/pvp-tier-list-ranked-arena-class-spec)")
                .WithColor(Color.DarkBlue)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("tournoi")]
        public async Task TournoiAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🎯 Prochains tournois WoW e-sport")
                .WithDescription("Suivez les événements compétitifs !")
                .AddField("🔴 Live & archives", "[YouTube AWC](https://www.youtube.com/user/WoWEsports)")
                .AddField("📅 Calendrier", "[Blizzard Arena Schedule](https://worldofwarcraft.blizzard.com/en-us/esports)")
                .WithColor(Color.Red)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("métier")]
        public async Task MetierAsync([Remainder] string metier)
        {
            var url = $"https://www.wow-professions.com/{Uri.EscapeDataString(metier.ToLower())}-guide";

            var embed = new EmbedBuilder()
                .WithTitle($"🔨 Guide métier : {metier}")
                .WithDescription("Guide complet pour monter le métier efficacement.")
                .WithUrl(url)
                .WithColor(Color.Orange)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("lieuxrp")]
        public async Task LieuxRpAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("🎭 Lieux RP emblématiques")
                .WithDescription("Envie de jouer un elfe noble ou un troll mystique ?")
                .AddField("🌲 Bois de la pénombre", "Ambiance gothique idéale.")
                .AddField("⚙️ Quartier de Forgefer", "RP nain, tavernes et forges.")
                .AddField("🪶 Cabestan", "Parfait pour du RP pirate/marchand.")
                .WithColor(Color.Purple)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("livreswow")]
        public async Task LivresWowAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("📚 Romans officiels de WoW")
                .WithDescription("Plonge dans le lore avec ces récits :")
                .AddField("• Arthas: Rise of the Lich King", "[Amazon](https://www.amazon.fr/dp/1416550947)")
                .AddField("• Illidan", "[Amazon](https://www.amazon.fr/dp/0399177562)")
                .AddField("• War Crimes", "[Amazon](https://www.amazon.fr/dp/1451684482)")
                .WithColor(Color.Teal)
                .Build();

            await ReplyAsync(embed: embed);
        }
        [Command("astrologie")]
        public async Task AstrologieAsync()
        {
            string[] signes = { "Soleil de Hurlevent", "Lune de Lune-d’Argent", "Éclipse d’Ahn’Qiraj" };
            string[] predictions = {
        "Une grande aventure vous attend dans les Terres Ingrates.",
        "L’amour frappera à la porte de votre guilde.",
        "Faites attention à votre équipement, il pourrait se briser bientôt !"
    };

            var r = new Random();
            await ReplyAsync($"🔮 Signe : **{signes[r.Next(signes.Length)]}**\n📘 Prophétie : *{predictions[r.Next(predictions.Length)]}*");
        }
        [Command("playlistwow")]
        public async Task PlaylistWowAsync()
        {
            await ReplyAsync("🎼 Playlist épique WoW sur YouTube :\nhttps://www.youtube.com/watch?v=DSUIhVAeTHQ&list=PLRQGRBgN_EnT1wYVbGyxPlFD3XrOrH-5r");
        }
        [Command("histoirejour")]
        public async Task HistoireJourAsync()
        {
            var histoires = new[]
            {
        "📜 *Il était une fois un orc nommé Grommash Hurlenfer...*",
        "📜 *Sylvanas Windrunner ne craignait rien... sauf le vide en elle.*",
        "📜 *Le vol draconique noir complotait dans les profondeurs...*"
    };
            await ReplyAsync(histoires[new Random().Next(histoires.Length)]);
        }
        [Command("siteofficiel")]
        public async Task SiteOfficielAsync()
        {
            await ReplyAsync("🌐 [Site officiel World of Warcraft](https://worldofwarcraft.blizzard.com/)");
        }

        [Command("boutique")]
        public async Task BoutiqueAsync()
        {
            await ReplyAsync("🛒 Boutique Blizzard :\nhttps://shop.battle.net/");
        }

        [Command("support")]
        public async Task SupportAsync()
        {
            await ReplyAsync("🛠️ Support officiel :\nhttps://eu.battle.net/support/fr/");
        }

        [Command("forums")]
        public async Task ForumsAsync()
        {
            await ReplyAsync("💬 Forums communautaires :\nhttps://eu.forums.blizzard.com/fr/wow/");
        }

    }
}
