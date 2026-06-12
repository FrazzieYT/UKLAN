# 🛑 ULTRAKILL LAN Co-op (UKLAN) — Project Archived

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Status: Archived](https://img.shields.io/badge/Status-Archived-red.svg)]()
[![BepInEx](https://img.shields.io/badge/BepInEx-5.4.21+-blue.svg)](https://github.com/BepInEx/BepInEx)

**⚠️ This project is no longer actively maintained. / ⚠️ Проект больше не поддерживается.**

The development has been paused. The code is provided "as is" for educational purposes and as a foundation for anyone who wants to fork and continue development.  
Разработка приостановлена. Код предоставляется "как есть" в образовательных целях и как основа для всех, кто хочет сделать форк и продолжить разработку.

---

## 🚦 Current Status / Текущее состояние

### ✅ What Works / Что работает
- **Network Core:** Client-server architecture based on LiteNetLib is functional.
- **Connection:** Players can host and join games via LAN.
- **In-Game Menu:** Press `F1` to open the UI, enter IP/port, and connect.
- **Basic Packet System:** Damage, death, and weapon fire events are transmitted.
- **Weapon Visuals:** Revolver beam effects are synchronized between players.
- **Player List:** The menu shows connected players with HP and ping.

### ✅ Что работает
- **Сетевое ядро:** Клиент-серверная архитектура на LiteNetLib функционирует.
- **Подключение:** Игроки могут создавать и присоединяться к играм по LAN.
- **Внутриигровое меню:** Нажмите `F1`, чтобы открыть UI, ввести IP/порт и подключиться.
- **Базовая система пакетов:** События урона, смерти и выстрелов передаются.
- **Визуалы оружия:** Эффекты лучей револьвера синхронизируются между игроками.
- **Список игроков:** В меню отображаются подключённые игроки с HP и пингом.

---

### ❌ What Doesn't Work / Что не работает
- **Player Capsules:** Remote player capsules are not spawned (need to create capsule or V1/V2 model).
- **Enemy Synchronization:** Enemies are not synced between players.
- **Position Interpolation:** Has issues with initial position (0,0,0) and smooth movement.
- **Advanced Features:** Ammo sync, full animation sync, and item pickup sync are not implemented.

### ❌ Что не работает
- **Капсулы игроков:** Капсулы удалённых игроков не создаются.
- **Синхронизация врагов:** Враги не синхронизируются между игроками.
- **Интерполяция позиции:** Есть проблемы с начальной позицией (0,0,0) и плавным движением.
- **Продвинутые функции:** Синхронизация патронов, полной анимации и подбора предметов не реализована.

---

### 🔧 Known Issues / Известные проблемы
- Some network packets may cause desync (garbage IDs in logs).
- The mod is optimized for 2 players only.

### 🔧 Известные проблемы
- Некоторые сетевые пакеты могут вызывать рассинхронизацию (мусорные ID в логах).
- Мод только для 2 игроков.

---

## 🤝 Want to Continue Development? / Хочешь продолжить разработку?

**Forks are highly encouraged!** 🍴

If you want to pick up this project:
1. **Fork** this repository.
2. **Fix the capsule bug** in `PlayerEntity.Read()` — create a capsule or implement the V1/V2 model.
3. **Implement enemy sync** — the `EnemyEntity` class is a starting point.
4. **Credit the original author** — keep the attribution to `FrazzieYT` as required by the MIT License.

If you make something cool, feel free to share it in the [Issues](https://github.com/FrazzieYT/UKLAN/issues) or let me know!

### 🤝 Хочешь продолжить разработку?

**Форки настоятельно приветствуются!** 🍴

Если хочешь подхватить этот проект:
1. **Сделай форк** этого репозитория.
2. **Исправь баг с капсулами** в `PlayerEntity.Read()` — сделай создание капсулы или модельки V1, V2.
3. **Реализуй синхронизацию врагов** — класс `EnemyEntity` является отправной точкой.
4. **Укажи оригинального автора** — сохрани упоминание `FrazzieYT`, как того требует лицензия MIT.

Если сделаешь что-то крутое — поделись в [Issues](https://github.com/FrazzieYT/UKLAN/issues) или дай знать!

---

## 📥 Installation / Установка

1. Install [BepInEx 5](https://github.com/BepInEx/BepInEx/releases) into your ULTRAKILL directory.
2. Download the latest release (if available) or build from source.
3. Copy `UltraLANCoop.dll` and `LiteNetLib.dll` into `ULTRAKILL/BepInEx/plugins/`.
4. Launch the game and press **F1**.

### Установка
1. Установите [BepInEx 5](https://github.com/BepInEx/BepInEx/releases) в папку с ULTRAKILL.
2. Скачайте последний релиз (если доступен) или соберите из исходников.
3. Скопируйте `UltraLANCoop.dll` и `LiteNetLib.dll` в `ULTRAKILL/BepInEx/plugins/`.
4. Запустите игру и нажмите **F1**.

---

## 🔨 Building from Source / Сборка из исходников

### For Developers / Для разработчиков
- **Visual Studio 2022** (or any C# IDE)
- **.NET Framework 4.8 SDK**

### Required DLLs / Необходимые DLL
Place these DLLs from `ULTRAKILL/ULTRAKILL_Data/Managed/` into `lib/local/`:

Поместите эти DLL из папки `ULTRAKILL/ULTRAKILL_Data/Managed/` в `lib/local/`:
- `BepInEx.dll`
- `0Harmony.dll`
- `Assembly-CSharp.dll`
- `UnityEngine.dll`
- `UnityEngine.CoreModule.dll`
- `UnityEngine.PhysicsModule.dll`
- `UnityEngine.IMGUIModule.dll`
- `UnityEngine.InputLegacyModule.dll`

---

## 📋 Requirements / Требования
- **ULTRAKILL** (Latest version recommended / рекомендуется последняя версия)
- **[BepInEx 5.4.21](https://github.com/BepInEx/BepInEx/releases)** or newer / или новее

---

## 📜 Attribution / Упоминание

🇬🇧 If you use this code or are inspired by it, please mention `FrazzieYT` with a link to the [original repository](https://github.com/FrazzieYT/UKLAN) in the Credits section of your project. This is not a strict MIT requirement, but it is highly appreciated in the community.

🇷🇺 Если вы используете этот код или вдохновляетесь им, пожалуйста, укажите `FrazzieYT` со ссылкой на [оригинальный репозиторий](https://github.com/FrazzieYT/UKLAN) в разделе Credits вашего проекта. Это не строгое требование MIT, но очень ценится в сообществе.

---

## 🤝 Credits & Благодарности

- **Original Author & Repository:** [FrazzieYT](https://github.com/FrazzieYT)
- **[LiteNetLib](https://github.com/RevenantX/LiteNetLib)** by RevenantX (Networking)
- **[BepInEx](https://github.com/BepInEx/BepInEx)** (Modding Framework)
- **[Harmony](https://github.com/pardeike/Harmony)** (Runtime Patching)

---

## 📜 License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.  
Этот проект распространяется под лицензией MIT – подробности см. в файле [LICENSE](LICENSE).

---

## 🔗 Links / Ссылки

- 📦 **[GitHub Repository / Репозиторий](https://github.com/FrazzieYT/UKLAN)**
- 🎮 **[ULTRAKILL Official Discord](https://discord.gg/ultrakill)**
- 🔧 **[BepInEx](https://github.com/BepInEx/BepInEx)**
- 🌐 **[LiteNetLib](https://github.com/RevenantX/LiteNetLib)**