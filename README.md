# 🧙 WoWGameInfo – Discord Bot pour World of Warcraft

**WoWGameInfo** est un bot Discord communautaire dédié à l’univers de **World of Warcraft**, toutes extensions confondues (Classic, Retail, MoP, etc.). Il fournit quiz, actualités, guides PvE, lore, événements, RSS, farming, et bien plus encore via des **commandes classiques** et **Slash Commands UX-Friendly** !

---

## ✨ Fonctionnalités principales

### 🎮 Quiz & Classements
- `!quiz` – Lance un quiz aléatoire sur WoW
- `!reponse <ta_réponse>` – Tente ta chance
- `!points`, `!classement` – Scores et top joueurs

### 📘 Lore & Histoire
- `/lore-encyclopedia <sujet>` – Exploration approfondie du lore via Wowpedia
- `/boss`, `/zones`, `/zone-info`, `/zone-activity` – Infos détaillées des zones
- `/lore-sites` – Liens utiles sur l’univers narratif de WoW

### 🛠️ Builds, Profils & Métiers
- `/build <classe>` – Guide PvE par classe
- `/build-auto <classe>` – Reco complète : talents, rotation, addons
- `/class-guide <classe> <spé>` – Guide spécialisé
- `/setup-profile` – Profil joueur personnalisé (RP, faction, classe)
- `/professions` – Liste et ressources métiers

### 🏰 Raids, Zones & Donjons
- `/boss`, `/zone-explorer` – Infos RP, activités contextuelles
- `/events`, `/world-event` – Activités mondiales en jeu

### 🎥 Vidéos & Actus
- `/youtube` – Chaînes utiles (guides, lore, actu)
- `/newsrss` – Dernières news via flux RSS (BlueTracker, Wowhead)
- `/menu` – Interface complète avec liens et raccourcis

### 🌿 Farming & Ressources
- `!routefarm <ressource>` – Conseils de farming
- `/zone-activity <zone>` – Activité contextuelle dynamique

---

## 🚀 Slash Commands & UI Interactive

- `/help` – Menu général avec catégories (Quiz, Lore, Builds, etc.)
- Boutons : 🎮 Quiz | 📘 Lore | 🧠 Builds | 🏰 Raids | 🎥 Vidéos
- Menus dynamiques (buttons, embeds riches, ephemeral UI)

---

## 💎 Exemples de Slash Commands Avancées

- `/setup-profile` – Faction, classe, style de jeu
- `/lore-encyclopedia illidan` – Accès rapide à Wowpedia
- `/zone-explorer durotar` – RP zone immersive
- `/build-auto paladin` – Build complet auto avec liens
- `/class-guide druide équilibre` – Guide icy-veins précis

---

## 🧠 Technologies

- `.NET 8+` / `C#`
- [Discord.Net v3.12+](https://github.com/discord-net/Discord.Net)
- Prise en charge :
  - Commandes classiques (`!`)
  - Slash Commands (`/`)
  - UI Discord : Buttons, Menus, Embeds

---

## 🛠️ Installation

```bash
git clone https://github.com/ton-utilisateur/WowGameInfo.git
cd WowGameInfo
dotnet restore
dotnet run
