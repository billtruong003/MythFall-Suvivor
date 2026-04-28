# 🏛️ GAME DESIGN — Mythfall: Survivors

> **Comprehensive game design document.**
> Vertical slice (2 tuần) chỉ implement core combat + 2 chars. File này là **full vision** để Claude Code understand context và build foundation expandable cho full game.

---

## 📑 TABLE OF CONTENTS

1. [Vision & Pitch](#1-vision--pitch)
2. [World Lore](#2-world-lore)
3. [Characters](#3-characters)
4. [Combat System](#4-combat-system)
5. [Progression Systems](#5-progression-systems)
6. [Gacha & Currency](#6-gacha--currency)
7. [UI/UX Vision](#7-uiux-vision)
8. [Visual Direction](#8-visual-direction)
9. [Audio Direction](#9-audio-direction)
10. [Localization Plan](#10-localization-plan)
11. [Vertical Slice Scope](#11-vertical-slice-scope)
12. [Post-Slice Roadmap](#12-post-slice-roadmap)

---

# 1. VISION & PITCH

## 1.1 Pitch

> **"Một phàm nhân giữ được Thiên Mệnh Ấn cuối cùng, triệu hồi các huyền thoại đã ngã xuống ra chiến đấu chống lại Hư Vô đang nuốt chửng thế giới."**

**Genre:** 3D top-down 45° auto-shooter survival roguelite + gacha character collection RPG

**Platform:** Android (Google Play) primary → iOS sau 3 tháng

**Engine:** Unity 6.3.10f1 (URP)

**Target audience:** Mobile gamer 16-35, played Vampire Survivors / Survivor.io / Genshin Impact

## 1.2 Core Pillars

| Pillar | Implementation |
|---|---|
| **Combat Feel** | Auto-attack roguelite + hitstop + juicy VFX + screen shake |
| **Build Depth** | In-run upgrade cards + element fusion + skill scaling per character |
| **Collection** | Gacha 4★/5★ với ascension đến 6★ Đỏ, mỗi character distinct combat fantasy |
| **Mythology** | 9 lục địa với Thần Chủ riêng — Đông + Tây mythology mix (Vietnamese flavored names) |
| **Daily Hook** | Daily mission, free pull, BP, event — daily reason to login |

## 1.3 Inspiration

- **Vampire Survivors** — auto-attack core loop
- **Megabonk** — 3D top-down validated cho mobile
- **Survivor.io** — hybrid monetization
- **Genshin Impact** — character collection + constellation
- **Solo Leveling** — army-building (Elyra inspiration)
- **Hades** — combat clarity + character voice
- **Hollow Knight** — atmospheric melancholy

---

# 2. WORLD LORE

## 2.1 The World — Cửu Châu Thiên Cổ

Thế giới gồm **9 lục địa thần vực**, mỗi vùng từng do một **Thần Chủ** cai quản, ngăn cách bởi **Thiên Mạc** (the Veil).

| # | Lục địa | EN Name | Thần Chủ | Theme | Hiện trạng |
|---|---|---|---|---|---|
| 1 | **Bắc Thiên Cương** | Northern Reach | Lôi Hoàng (Thần Sấm Sét) | Băng giá, núi cao, bão tố | Trọng thương, ẩn náu |
| 2 | **Thanh Lâm Vực** | Verdant Realm | Lục Y Tiên Tử (Nữ Thần Rừng) | Rừng nguyên sinh, druid | Trọng thương, ẩn náu |
| 3 | **Hỏa Diễm Quốc** | Ember Dominion | Phụng Dực Đế (Hỏa Phượng) | Núi lửa, sa mạc đỏ | **Đã chết** |
| 4 | **Hàn Băng Đảo** | Frostshard Isles | Bạch Sương Nương (Băng Hậu) | Quần đảo băng, ngư tộc | **Đã chết** |
| 5 | **Hoàng Sa Thành** | Goldsand Empire | Nhật Quang Vương (Thần Mặt Trời) | Sa mạc, đền tháp | **Đã chết** |
| 6 | **U Minh Cốc** | Shadowfen Valley | Hắc Liêm Tôn (Tử Thần) | Đầm lầy, ma thuật cấm | **Mất tích** |
| 7 | **Cửu Thiên Sơn** | Cloudpiercer Peaks | Vân Tử Tiên Sư (Thần Gió) | Núi mây, tu viện, kiếm sư | Trọng thương, ẩn náu |
| 8 | **Hồng Hải Vực** | Crimson Tides | Long Khôi Vương (Hải Long Đế) | Biển đỏ, cướp biển | **Đã chết** |
| 9 | **Hư Linh Cảnh** | Voidmirror Reach | *(?)* | Vùng đất chết | **Sụp đổ TRƯỚC Thiên Kiếp** |

**Mystery hook:** Lục địa thứ 9 sụp đổ **trước** Thiên Kiếp 50 năm. Nguyên nhân không ai biết — đây là đầu mối lớn xuyên suốt story arc.

## 2.2 Thiên Kiếp Đại Lạc — The Sundering

**327 năm trước**, Thiên Mạc bị xé toạc trong **Thất Dạ Phá Thiên** (7 ngày 7 đêm tan vỡ vũ trụ).

**Hậu quả:**
- 4/8 Thần Chủ tử vong
- 3 Thần Chủ trọng thương phải ẩn náu
- 1 Thần Chủ mất tích (Hắc Liêm Tôn — giả thuyết: chính ông gây ra Thiên Kiếp)
- 200+ Á Thần (cận thần / hậu duệ) chết hoặc bị phong ấn
- **Hư Vô** tràn vào qua vết nứt

## 2.3 Hư Vô — The Void Hollow (Antagonist)

**Định nghĩa cụ thể:**
- Vật chất màu xám nhạt, di chuyển như khói nhưng có khối lượng
- Khi tiếp xúc sinh vật → ăn theo thứ tự: **ký ức → cảm xúc → linh hồn**
- Sinh vật bị Hư Vô hoá hoàn toàn = **Hư Linh Quỷ** (Voidborn enemies in game)
- KHÔNG có ý chí trung tâm — là hiện tượng vật lý của vũ trụ tan vỡ
- Càng gần Anchor, càng "đặc" và "thông minh" hơn — như đang học (foreshadowing)

**Late-game antagonist face:**
- **"Hư Linh Quân Vương"** (Void Sovereign) — figurehead reveal late, có thể là Hắc Liêm Tôn đã biến chất

## 2.4 Thánh Hỏa Đài — The Anchors

**12 Anchors ban đầu, hiện còn 5:**

1. **Bắc Phong Thánh Đài** — phía bắc, hậu duệ Lôi Hoàng giữ
2. **Hắc Liễu Thánh Đài** — sâu trong U Minh Cốc, vô chủ, tự duy trì
3. **Vô Tận Hỏa Đài** — trên núi lửa, Phụng tộc còn sót lại
4. **Vân Đài** — trên Cửu Thiên Sơn, tu viện Vân Tử bảo vệ
5. **Sơ Lai Đài** (The First Hearth) — **player home** — nhỏ nhất, ở rìa Thanh Lâm Vực

**Sơ Lai Đài đặc biệt:**
- Xây bởi phàm nhân trong Thiên Kiếp, không có Thần Chủ bảo trợ
- Không có Sacred Flame thật — dùng **Thiên Mệnh Ấn** của player tạo "ngọn lửa giả"
- Là lý do player có thể triệu hồi Echoes từ MỌI lục địa (trung lập)

## 2.5 Player — Thiên Mệnh Chi Chủ

Player là **một phàm nhân** đã tìm thấy **Thiên Mệnh Ấn** trong tàn tích sau Thiên Kiếp.

**Thiên Mệnh Ấn:**
- Theo truyền thuyết, là vật phẩm "trọng tài" giữa các Thần Chủ thời tiền sử
- Cho phép người mang **triệu hồi** và **đánh giá** thực thể thần linh
- Đây là tại sao có hệ sao 4★→6★ — **cấp độ thiên mệnh** mà ấn đo được

**Player KHÔNG có combat ability cá nhân** — chỉ là người chỉ huy. Đó là gameplay justification: mỗi run chọn 1 hero, hero chiến đấu thay player.

## 2.6 Story Arc Overview

| Act | Chapters | Theme |
|---|---|---|
| **Awakening** | Ch 1-3 | Player phát hiện Thiên Mệnh Ấn có sức mạnh, triệu hồi heroes đầu, hiểu Hư Vô threat |
| **Investigation** | Ch 4-6 | Tìm hiểu nguyên nhân Thiên Kiếp, khám phá Hư Linh Cảnh, gặp các Thần Chủ còn sống |
| **Confrontation** | Ch 7-9 | Đối mặt Hư Linh Quân Vương, reveal nguyên nhân Thiên Kiếp, cuộc chiến cuối |

---

# 3. CHARACTERS

## 3.1 Star System

```
3★         →  Material only (NOT playable in main run)
4★ → +5    →  Eligible for 5★ ascension
5★ → +5    →  Eligible for 6★ Đỏ ascension
6★ Đỏ      →  Final tier
```

**Why 3★ not playable:**
- Single character per run game design
- Tránh feeling "spam rác" trong inventory
- 3★ là fodder & material — feed vào 4★/5★ để ascend
- Lore: 3★ là thường dân/lính thường, không đủ thiên mệnh dùng làm hero

## 3.2 4★ Roster (10 characters)

### 4★-1. Kai Lôi Phong — *"Cô Lang"*

**Origin:** Bắc Thiên Cương, hậu duệ Lôi Hoàng (cách 7 đời)

**Lore:** Con trai út tộc trưởng Lôi Phong. Khi Hư Vô đến, các tộc lớn đóng cửa thành không cho Lôi Phong tị nạn. Cả tộc bị tiêu diệt 3 ngày. Kai sống sót vì lúc đó đi săn xa. **Mang oán hận không chỉ Hư Vô — mà cả các tộc đã từ chối cứu tộc cậu.**

**Visual:** Tóc đỏ nâu, scar dọc má phải (tự rạch), áo da sói, song kiếm (kiếm dài di vật cha — vẫn vết máu cha; kiếm ngắn lấy từ kẻ thù đầu tiên), tattoo runic glow đỏ khi rage.

**Voice:** Low, gruff. Battle cries thay câu nói dài.

**Combat fantasy:** *"Càng đau càng khoẻ, cuối cùng hóa cơn bão sống."*

**Skills:**

| Tier | Skills |
|---|---|
| 4★ Lv1 | Auto: chém song kiếm. Active 1: **Lôi Bộc** (lao 8m, damage path). Passive: HP < 50% → ATK +30% |
| 4★+3 | Lôi Bộc: enemy bị đánh dấu **Lôi Tích** (stack 3 = nổ sét nhỏ) |
| 4★+5 | Active 2: **Hắc Vũ** (mỗi 10s sét tự rơi xuống enemy gần nhất) |
| 5★ awaken | Lôi Bộc: 2 charge. Passive: HP < 30% → ATK +60% + lifesteal 5% |
| 5★+5 | Hắc Vũ: 3 enemy auto, mỗi 6s |
| 6★ Đỏ | **Lôi Đình Hóa Hình** — HP < 20% → avatar sét 8s, immortal + sét 0.5s |

---

### 4★-2. Lyra Vọng Nguyệt — *"Kẻ Phản Đồ"*

**Origin:** Thanh Lâm Vực, dòng dõi Tế Nguyệt (phụng sự Lục Y Tiên Tử)

**Lore:** **Phát hiện trưởng lão tộc thông đồng với Hư Vô** — đổi dân thường lấy "trí tuệ cấm". Tố cáo, bị xử tử hình, trốn thoát. Thiên Kiếp xảy ra — chính trưởng lão mở cổng cho Hư Vô. Mang gánh nặng *"nếu mình tố cáo sớm hơn..."*

**Visual:** Tóc bạch kim dài chấm hông, mắt xanh ngọc có quầng mệt mỏi (300+ năm lưu vong), áo choàng xanh lục đậm với vết khâu lại nơi từng có huy hiệu, cung dài bằng gỗ Eldwood.

**Voice:** Soft, precise, melodic. Sing-songy khi cast spell.

**Combat fantasy:** *"Mỗi mục tiêu là một câu trả lời cho tội lỗi của tộc tôi."*

**Skills:**

| Tier | Skills |
|---|---|
| 4★ Lv1 | Auto: tên xuyên thấu. Active 1: **Nguyệt Quang Tiễn** (charge 1s, beam xuyên). Passive: hit cùng enemy → stack Nguyệt Ấn (max 4, +20% damage) |
| 4★+3 | Nguyệt Ấn 4 stacks → enemy đánh dấu, mọi attack +30% damage |
| 4★+5 | Active 2: **Nguyệt Vịnh** (slow toàn enemy + tăng tốc bắn 50% trong 5s) |
| 5★ awaken | Charge giảm còn 0.5s, beam xuyên vô hạn |
| 5★+5 | Nguyệt Ấn stack mới chỉ 2 hit |
| 6★ Đỏ | **Tử Vong Khúc** — tên cuối có thể "thoái hồi" từ xác enemy chết, max 5 lần dội |

---

### 4★-3. Akari Mộc Linh Hồ — *"Tên Trộm"*

**Origin:** Hỏa Diễm Quốc, vùng núi lửa Sakura no Hi

**Lore:** Tên trộm bùa chú nổi tiếng. Khi Thiên Kiếp xảy ra, đang trộm đền của hồ ly cổ **Yamabuki**. Trộm thành công nhưng ngọc vỡ — **hồn hồ ly nhập vào Akari**. Giờ chia sẻ thân thể với hồ ly 800 tuổi rất phiền phức.

**Visual:** Tóc nâu đỏ búi cao, **tai hồ ly mọc khi cast spell**, kimono cắt ngắn, 3 đuôi hồ ly lửa (2 + 1 của Yamabuki), mắt chuyển hổ phách khi rage.

**Combat fantasy:** *"Một thân, hai hồn, lửa rực trời."*

**Đặc biệt:** In-game thỉnh thoảng có voice line đối thoại — Akari + Yamabuki cãi nhau.

**Skills:**

| Tier | Skills |
|---|---|
| 4★ Lv1 | Auto: ném bùa lửa AoE. Active 1: **Hồ Hỏa Phân Thân** (Yamabuki tách ra clone tự attack 5s) |
| 4★+3 | Phân Thân: gây thêm Burn DoT |
| 4★+5 | Active 2: **Cửu Vĩ Thiêu** (mọc đủ 9 đuôi, 10s tăng cast speed 100%) |
| 5★ awaken | 2 clone đồng thời, AI riêng |
| 5★+5 | Burn DoT spread sang enemy gần khi target chết |
| 6★ Đỏ | **Yamabuki Hiện Thân** — Yamabuki chiếm thân 10s, toàn map cháy |

---

### 4★-4. Lưu Tam Liên — *"Sát Thủ Tự Lưu Vong"*

**Origin:** Cửu Thiên Sơn, tông môn Hắc Vân Quan

**Lore:** **Tông môn không bị Hư Vô diệt — tự diệt chính mình.** Trưởng môn tranh đoạt mảnh thần linh, đầu độc, ám sát lẫn nhau. Liên là đệ tử trẻ nhất, từ chối tham gia, bị coi "phản đồ", chạy trốn. Mang theo bí ẩn về **mảnh thần linh đáng giá đến mức cả tông môn tự sát.**

**Visual:** Tóc đen ngắn ngang vai, **thân hình rất gầy** (50 năm tự kỷ luật), trang phục xám đen, không vũ khí, tattoo Hắc Vân Quan trên cổ tay phải đã cố cào đi nhưng không sạch.

**Combat fantasy:** *"Bàn tay trần là vũ khí cuối cùng còn tin được."*

**Skills:**

| Tier | Skills |
|---|---|
| 4★ Lv1 | Auto: 3-hit combo. Active 1: **Bóng Bước** (teleport 3m, next attack crit) |
| 4★+3 | Combo hit thứ 3 → AoE shockwave |
| 4★+5 | Active 2: **Tịch Diệt Quyền** (5s next attacks 200% damage + crit guaranteed) |
| 5★ awaken | Bóng Bước: 2 charge |
| 5★+5 | Tịch Diệt Quyền: 8s, kết thúc → AoE explosion |
| 6★ Đỏ | **Vô Hình Sát** — vô hình với enemy AI 6s, mọi hit là crit |

---

### 4★-5. Aurelia di Vetrano — *"Công Chúa Lưu Vong"*

**Origin:** Hồng Hải Vực, vương quốc Vetrano (sụp 30 năm trước Thiên Kiếp)

**Lore:** **Phụ vương bán đứng vương quốc cho Hư Vô** đổi quyền năng cá nhân. Aurelia phát hiện trước lễ đăng quang 18 tuổi, bỏ trốn cùng druid Brynn. Vương quốc sau đó bị Hư Vô nuốt — phụ vương cũng bị phản bội. **Tự trách vì không phát hiện sớm để cứu mẫu hậu.**

**Visual:** Tóc vàng dài thắt vương miện gai (làm từ thực vật), mắt xanh lá, áo choàng nâu lục thay hoàng bào, **đeo nhẫn signet Vetrano** (ám ảnh quá khứ), gậy gỗ là di vật của Brynn.

**Combat fantasy:** *"Rừng nhớ — và rừng phán xét."*

**Skills:**

| Tier | Skills |
|---|---|
| 4★ Lv1 | Auto: gai mọc lên đâm enemy. Active 1: **Gốc Phán Quyết** (rễ trồi lên, root 2s) |
| 4★+3 | Gốc: thêm poison DoT |
| 4★+5 | Active 2: **Sinh Mệnh Triệu Hồi** (gọi 3 cây con tự attack 8s) |
| 5★ awaken | Cây con: range attack thay melee |
| 5★+5 | Cây con khi chết → nổ poison cloud |
| 6★ Đỏ | **Lục Y Hiện Thân** — gọi 1 cây cổ thụ khổng lồ chiến đấu cùng 12s |

---

### 4★-6. Marcus Aurelius Valtor — *"Tướng Quân Thất Bại"*

**Origin:** Hoàng Sa Thành, đế quốc Valtor (đã phế tích)

**Lore:** Đại tướng quân thắng 47 trận liên tiếp — "Sư Tử Của Cát". **Trận thứ 48 — quan trọng nhất — thua. Hư Vô tràn qua phòng tuyến, phá hủy Anchor đầu tiên. 200,000 người chết.** Sống sót vì lính đẩy ra khỏi chiến trường. **Chuộc lỗi không thể chuộc.**

**Visual:** Tóc đen pha bạc thái dương (35 tuổi nhưng lão hóa do guilt), bộ giáp Roman sờn cũ — chưa sửa từ trận 48 (vết nứt còn), **khiên đồng khắc tên 200,000 người chết** (vẫn cập nhật).

**Combat fantasy:** *"Đứng giữa, để không ai phải đứng phía sau lần nữa."*

**Skills:**

| Tier | Skills |
|---|---|
| 4★ Lv1 | Auto: đẩy khiên. Active 1: **Tường Đồng** (barrier 360°, hấp thụ damage 4s) |
| 4★+3 | Barrier: damage hấp thụ → ATK boost 5s sau |
| 4★+5 | Active 2: **Khiên Bão** (dash forward, đẩy enemy, stun 1s) |
| 5★ awaken | Tường Đồng: reflect 30% damage |
| 5★+5 | Khiên Bão: 2 charge |
| 6★ Đỏ | **Sư Tử Phục Sinh** — 1 lần per run, chết → revive 50% HP + immunity 5s |

---

### 4★-7. Soren Cảnh Phong — *"Tên Tù Trốn Trại"*

**Origin:** Cửu Thiên Sơn, không tông môn

**Lore:** Bị tù vì giết quý tộc khi 19 tuổi (quý tộc hãm hiếp em gái — tòa không tin). Trong tù gặp lão già không tên dạy Vân Tử Phong Kiếm. Trốn ra cùng lão già khi Hư Vô tấn công nhà tù — lão chết trên đường. **Vẫn bị truy nã (mặc dù đế quốc đã sụp).** Không có grand plan — chỉ chạy.

**Visual:** Tóc đen rối, **scar thắt hình chữ thập trên cổ tay** (dấu tù — không che), áo vải thô xám, kiếm dài đơn — **không có vỏ kiếm**, dáng "sẵn sàng chạy".

**Combat fantasy:** *"Lưỡi gió không bao giờ chậm — vì kẻ chậm đã chết rồi."*

**Skills:**

| Tier | Skills |
|---|---|
| 4★ Lv1 | Auto: 4-hit combo gió. Active 1: **Trảm Phong** (dash + slash, vệt gió damage thêm) |
| 4★+3 | Vệt gió tồn tại 3s, enemy đi qua bị damage |
| 4★+5 | Active 2: **Bão Cuồng** (quay tròn 2s, lốc nhỏ AoE 5m) |
| 5★ awaken | Trảm Phong: 3 charge, dash xuyên enemy |
| 5★+5 | Bão Cuồng: kéo enemy về center |
| 6★ Đỏ | **Vân Tử Tâm Pháp** — flow state 8s, attack hit 2 lần, dodge auto |

---

### 4★-8. Kestrel — *"Vũ Khí Của Tổ Chức"*

**Origin:** Không rõ — Kestrel không nhớ nơi sinh

**Lore:** Đứa trẻ bị bỏ rơi, được tổ chức ngầm **Quervo** nhặt về huấn luyện sát thủ từ năm 4 tuổi. Không biết tên thật, không biết cha mẹ, không nhớ tuổi thật. "Kestrel" là code name. Khi Quervo tan rã trong Thiên Kiếp, được "tự do" lần đầu — và **không biết tự do là gì.** Tham gia với player vì player là người đầu tiên hỏi cô *"tên thật của em là gì?"*

**Visual:** Tóc bạc trắng (do trauma sớm), mặt baby face đối lập với eyes cold, **mặc đồ rộng** (không thích bị bó buộc), 2 dao găm, **không có scar** — Quervo không cho phép sát thủ có dấu nhận diện.

**Combat fantasy:** *"Bóng tối là người mẹ duy nhất tôi có."*

**Skills:**

| Tier | Skills |
|---|---|
| 4★ Lv1 | Auto: cắt nhanh 2 dao. Active 1: **Bóng Tối** (vô hình 2s, attack tiếp theo crit + backstab) |
| 4★+3 | Bóng Tối: 4s, thoát target lock |
| 4★+5 | Active 2: **Sát Lệnh** (đánh dấu 5 enemy gần, 3s sau guaranteed crit) |
| 5★ awaken | Bóng Tối: hồi đầy HP khi vào shadow |
| 5★+5 | Sát Lệnh: enemy chết → enemy gần nhất bị đánh dấu thay |
| 6★ Đỏ | **Vô Danh** — xóa khỏi reality 5s, không thể bị attack, attack bypass mọi defense |

---

### 4★-9. Torin Đông Lan — *"Hoàng Tử Không Mong Đợi"*

**Origin:** Bắc Thiên Cương, vương quốc nhỏ Đông Lan

**Lore:** Hoàng tử thứ hai — **không có quyền thừa kế**. Sống cuộc đời "spare" — uống rượu, đi săn, không trách nhiệm. Khi Thiên Kiếp xảy ra, anh trai chết, phụ vương trọng thương, **vương quốc cần lãnh đạo. Ép lên ngôi → từ chối, bỏ trốn → bị các trưởng lão coi kẻ phản bội.** Lưu vong, mang huyết thống Lôi Hoàng nhưng từ chối danh hiệu.

**Visual:** Tóc nâu vàng cắt ngắn, **luôn cầm bình rượu**, áo da quý tộc cũ, **vẫn đeo vương miện vàng nhỏ dưới áo** (không tháo nổi), cây trượng có ngọc sét (Lôi Trượng truyền đời, hắn coi "đồ chơi").

**Combat fantasy:** *"Sét yếu khi tôi yếu — và tôi luôn yếu cho đến khi không còn lựa chọn."*

**Đặc biệt — "Snowball Intensify":** Đầu run sét rất yếu, càng kill enemy càng mạnh. Cuối run = mưa sét toàn map.

**Skills:**

| Tier | Skills |
|---|---|
| 4★ Lv1 | Auto: laser sét nhỏ (yếu). Mỗi kill +1 **Lôi Tích** (max 99). Active 1: **Sét Phán** (cột sét, damage = base × Lôi Tích) |
| 4★+3 | Lôi Tích max 199 |
| 4★+5 | Active 2: **Lôi Vũ** (toàn screen mưa sét 5s, cần Lôi Tích > 50) |
| 5★ awaken | Lôi Tích max 499 |
| 5★+5 | Lôi Vũ: 10s, không tốn Lôi Tích |
| 6★ Đỏ | **Lôi Hoàng Hậu Duệ** — kết thúc run với Lôi Tích > 200 → next run inherit stack |

---

### 4★-10. Mei Phong Tỏa — *"Doanh Nhân Phá Sản"*

**Origin:** Hoàng Sa Thành, gia tộc thương nhân Mei

**Lore:** **Nữ doanh nhân — không phải priestess, không phải warrior.** Gia tộc Mei kiểm soát thị trường bùa chú lớn nhất. Hư Vô phá huỷ — mất sạch tài sản. Còn lại kiến thức + viên ngọc dự phòng. Bắt đầu từ 0 ở tuổi 32. **Không phải anh hùng — tham gia vì tin "cứu thế giới = thị trường lớn nhất".**

**Visual:** Tóc đen búi cao kiểu doanh nhân, kính lúp đeo cổ, **mặc kimono đỏ thêu vàng** (bộ duy nhất còn — biểu tượng vinh quang đã mất), 7 viên ngọc bùa đeo dây chuyền, tay luôn có sổ kế toán nhỏ.

**Combat fantasy:** *"Mỗi enemy chết là một đồng coin về túi tôi."*

**Đặc biệt — "Gold-based":** Earn extra gold trong run, gold có thể spend mid-run buff skill.

**Skills:**

| Tier | Skills |
|---|---|
| 4★ Lv1 | Auto: ném bùa nổ. Mỗi kill +5 gold. Active 1: **Phù Thuật Triệu Hồi** (spend 50 gold, gọi golem 8s) |
| 4★+3 | Phù Thuật: golem damage tăng theo gold (max 200 gold = 4x damage) |
| 4★+5 | Active 2: **Hỏa Lan Ấn** (spend 100 gold, AoE explosion) |
| 5★ awaken | Mei nhận double gold pickup |
| 5★+5 | Phù Thuật: 2 charge |
| 6★ Đỏ | **Tài Phú Hợp Đồng** — spend 1000 gold → triệu hồi **Vàng Long** tự fight 30s |

---

## 3.3 5★ Roster (7 characters)

### 5★-1. Elyra Bích Huyết — *"Nữ Vương Của Cõi Chết"*

**Origin:** U Minh Cốc, vương quốc cổ Bích Huyết (đã rơi vào Hư Vô)

**Lore:** Vợ của Á Thần (cận thần Hắc Liêm Tôn). Chồng chết trong Thiên Kiếp. Dùng huyết thuật cấm giữ linh hồn chồng trong viên ngọc. **327 năm tích đủ linh hồn để hồi sinh chồng.** Đã hiến tế dân chúng vương quốc Bích Huyết để có đủ linh hồn. Lưu vong, săn linh hồn quái vật.

**Motivation:** Hồi sinh chồng — bằng mọi giá. Sẵn sàng phản bội nếu có cơ hội tiếp cận linh hồn lớn.

**Visual:** Tóc đen tuyền dài chấm gót, da trắng lạnh lẽo, **mắt đỏ máu**, vương miện gai sắt đen, áo choàng đen-đỏ pattern xương rồng, **đeo viên ngọc đỏ ở cổ — đó là chồng cô**.

**Combat fantasy:** Necromancer army-builder (Sung Jin-Woo + Yorick).

**Skills:**

| Tier | Skills |
|---|---|
| 5★ Lv1 | Auto: phóng máu thanh. Active 1: **Triệu Thi** (gọi 3 zombie từ xác enemy gần, 10s). Passive: enemy chết → +1 stack Linh Hồn Tích |
| 5★+3 | Triệu Thi: zombie merge → 1 elite stronger nếu Linh Hồn Tích > 20 |
| 5★+5 | Active 2: **Huyết Nguyệt Triệu** (gọi **Huyết Thi Chỉ Huy** mini-boss đồng minh, 30s) |
| 6★ Đỏ | **Vĩnh Sinh Quân Đoàn** — mọi enemy chết tự động trở thành đồng minh trong 60s |

---

### 5★-2. Raijin Xích Thiên — *"Lôi Hoàng Tái Lâm"*

**Origin:** Bắc Thiên Cương — **CHÍNH LÀ Lôi Hoàng Thần Chủ**

**Lore:** Sau Thất Dạ Phá Thiên bị thương quá nặng, trú trong tảng băng đáy biển 327 năm tự chữa. Lần đầu tỉnh dậy là khi player triệu hồi. **Chỉ phục hồi 5% sức mạnh** — nhưng 5% Thần Chủ vẫn mạnh hơn 100% hậu duệ.

**Personality:** Cổ kính, trầm trọng, thường nhìn xa xăm như nhớ ai. Quý mến Kai (cùng huyết thống) — Kai không biết, chỉ thấy ông già này phiền.

**Visual:** Tóc bạc trắng dài đến gối, **vẫn còn vết thương lớn ở ngực** (chưa lành sau 327 năm), áo choàng vàng-đen sét, **bàn tay là vũ khí** (mỗi ngón = 1 thanh sét).

**Combat fantasy:** Storm sovereign — area sét massive.

**Skills:**

| Tier | Skills |
|---|---|
| 5★ Lv1 | Auto: **5 ngón tay phát sét đồng thời** (5 target). Active 1: **Phán Quyết Sét** (cột sét khổng lồ AoE). Passive: aura sét, enemy gần auto stunned |
| 5★+3 | Aura: radius +50% |
| 5★+5 | Active 2: **Lôi Đình Đại Trận** (9 cột sét tại 9 vị trí random) |
| 6★ Đỏ | **Lôi Hoàng Trở Lại** — phục hồi đầy 15s, toàn map sét, enemy tự chết khi spawn |

---

### 5★-3. Thiên Cơ Nguyệt — *"Vận Mệnh Nghịch Hành"*

**Origin:** Cửu Thiên Sơn — **chị gái của Nguyệt Thần đã chết**

**Lore:** Em gái (Nguyệt Thần) chết trong Thiên Kiếp khi cứu Lyra (yes — có liên kết, Lyra biết). **Buộc đảm nhận quyền năng em** — nhưng không sinh ra để làm Thần. Mỗi lần dùng quyền năng, linh hồn bào mòn — già đi nhanh chóng. **Tìm hậu duệ phù hợp để truyền quyền — Lyra là ứng viên hàng đầu (Lyra không biết).**

**Visual:** Tóc trắng bạc dài (đáng lẽ đen — bào mòn), **da nhăn nhưng mắt vẫn sáng**, áo choàng bạc-tím sao đêm, **mặt trăng nhỏ luôn xoay quanh đầu** (visible sign), gậy ngọc trăng.

**Combat fantasy:** Time/fate manipulator.

**Skills:**

| Tier | Skills |
|---|---|
| 5★ Lv1 | Auto: bắn ánh trăng AoE. Active 1: **Thủy Triều Phán** (kéo enemy vào center + damage). Passive: mỗi 30s dilate time 2s (slow 50%) |
| 5★+3 | Time dilate: 4s, slow 70% |
| 5★+5 | Active 2: **Vận Mệnh Đảo Ngược** (last 5s damage taken → reverse to enemy) |
| 6★ Đỏ | **Nguyệt Thần Thừa Kế** — pause time 8s, chỉ Thiên Cơ + allies di chuyển được |

---

### 5★-4. Drakon Hắc Lân — *"Cổ Long Nửa Thân"*

**Origin:** Hỏa Diễm Quốc — **LÀ một con rồng cổ đại 1,200 tuổi**

**Lore:** Trước Thiên Kiếp có thể biến hình giữa rồng và người. **Sau Thiên Kiếp kẹt trong hình nửa người nửa rồng** — không thể trở lại nguyên hình rồng, không thể hoàn toàn thành người. Cảm thấy không thuộc về thế giới nào.

**Personality:** Ngạo mạn cổ kính, "fish out of water" với cảm xúc human modern. Tôn trọng player vì player không sợ hắn.

**Visual:** Cao 2.2m, **vảy đen ngọc khắp cơ thể**, **2 sừng cong sau**, mắt vàng dài như rồng, **đôi cánh nhỏ không bay được** (tổn thương), không mặc áo trên (vảy bảo vệ), váy da đỏ.

**Combat fantasy:** Dragon berserker — flame + rage.

**Skills:**

| Tier | Skills |
|---|---|
| 5★ Lv1 | Auto: phun lửa đường thẳng. Active 1: **Long Hỏa Bộc** (AoE explosion, lửa cháy 5s). Passive: HP < 40% → ATK +50% |
| 5★+3 | Lửa cháy: spreads sang enemy gần |
| 5★+5 | Active 2: **Lân Bão** (bay lên 3s, dive bomb AoE massive) |
| 6★ Đỏ | **Cổ Long Hồi Thân** — lấy lại hình rồng nguyên thủy 12s |

---

### 5★-5. Hàn Tuyết Phong — *"Nữ Hoàng Đông Miên"*

**Origin:** Hàn Băng Đảo — **Nữ Hoàng thật sự**

**Lore:** Khi Thiên Kiếp xảy ra, **tự nguyện đóng băng toàn bộ quốc gia** để bảo vệ khỏi Hư Vô. Họ không chết, ngủ đông trong băng. **Cô là người duy nhất phải tỉnh** — mỗi 100 năm tỉnh 1 năm để duy trì phép, sau đó ngủ tiếp. **Lần thức dậy thứ 4** — đã 327 + 40 (sinh học). Mỗi lần tỉnh, yếu đi.

**Visual:** Tóc bạch kim rất dài, **da xanh nhạt** (do băng nhập thân), mắt xanh lam như băng, **frost halo quanh người**, váy băng tinh thể, **vương miện băng vĩnh cửu**.

**Combat fantasy:** Ice queen — freeze + shatter.

**Skills:**

| Tier | Skills |
|---|---|
| 5★ Lv1 | Auto: shard băng homing. Active 1: **Đông Hàn Trận** (toàn screen freeze 3s). Passive: enemy đứng yên 2s → tự đóng băng |
| 5★+3 | Đông Hàn Trận: enemy frozen → shatter 50% HP khi tan |
| 5★+5 | Active 2: **Băng Đường** (đường băng dài 15m, enemy đi vào auto frozen) |
| 6★ Đỏ | **Vĩnh Đông** — toàn battlefield freeze 20s, enemy spawn = frozen |

---

### 5★-6. Viktor Cố Tu Sĩ — *"Tiền Nhiệm Thiên Mệnh"*

**Origin:** Không rõ — **Viktor TỪNG là Thiên Mệnh Chi Chủ trước player** (200 năm trước)

**Lore:** Mang Thiên Mệnh Ấn trước player. **Đã thất bại** — toàn bộ team Echoes hắn triệu hồi đều chết trong trận chiến quyết định với Hư Vô. **Không thể chết do Thiên Mệnh Ấn bảo vệ holder**, nhưng linh hồn hắn bị tổn thương vĩnh viễn. Khi player nhặt Ấn, ấn truyền sang. Viktor sống 200 năm với guilt. Khi player đủ trưởng thành, Viktor xuất hiện như **mentor figure**.

**Motivation:** Đảm bảo player không lặp lại sai lầm hắn. Mentor pure + warning living.

**Visual:** Tóc bạc dài tới vai, **râu ngắn bạc**, áo choàng đen với chữ rune cũ phai (artifact Thiên Mệnh Ấn cũ), **2 thanh kiếm đen ngược trên lưng** (kiếm của 2 thành viên team hắn — di vật), tuổi visual ~50 nhưng thực 230+.

**Combat fantasy:** Soul harvester + mentor figure.

**Skills:**

| Tier | Skills |
|---|---|
| 5★ Lv1 | Auto: 2 kiếm song chém. Active 1: **Hồn Khí Thu** (hấp thụ linh hồn enemy chết, +5% all stats 10s). Passive: stats scale theo enemy giết trong run (mỗi 100 kill = +1% all stats permanent for run) |
| 5★+3 | Hồn Khí Thu: stack max 5, +25% all stats |
| 5★+5 | Active 2: **Linh Hồn Đoán Lệnh** (gọi 5 linh hồn allies quá khứ chiến đấu cùng 15s) |
| 6★ Đỏ | **Tiền Nhiệm Trở Về** — vô địch + revive 5 fallen allies (linh hồn quá khứ) trong 20s |

---

### 5★-7. Hiroshi Bóng Tối — *"Người Sống Trong Hư Vô"*

**Origin:** **Hư Linh Cảnh — lục địa thứ 9** đã sụp đổ trước Thiên Kiếp

**Lore:** Á Thần của Hư Linh Cảnh. Khi Hư Vô tràn vào, không chạy — **học cách điều khiển Hư Vô**. Người duy nhất biết cách "fight fire with fire". **Mỗi lần dùng quyền năng, bị nhiễm thêm Hư Vô.** Đang dần biến thành thứ hắn ghét nhất.

**Motivation:** Tìm cách đảo ngược biến đổi — hoặc tìm ai đó có thể giết hắn khi hắn hoàn toàn biến chất. Player có thể là người làm việc đó.

**Visual:** **Một bên cơ thể là Hư Vô** — bóng tối khói xám dày đặc thay da. Nửa kia là human. **Mắt phải đen tuyền, mắt trái xanh nhạt**. Áo choàng trắng-đen split. Kiếm dài lưỡi đen như bóng tối. **Càng gần endgame, phần Hư Vô càng lan rộng** (mechanically tracked).

**Combat fantasy:** Void manipulator — cắt không gian.

**Skills:**

| Tier | Skills |
|---|---|
| 5★ Lv1 | Auto: chém kiếm đen, lưỡi xuyên không gian. Active 1: **Hư Không Trảm** (teleport behind enemy, slash + return, damage 2 lần). Passive: dùng skill → +1 stack Hư Vô (visual: Hư Vô lan rộng) |
| 5★+3 | Hư Không Trảm: 3 charge |
| 5★+5 | Active 2: **Vô Tận Cắt** (cắt không gian thành mảnh, AoE damage + DoT 5s) |
| 6★ Đỏ | **Hư Vô Thừa Nhận** — biến thành **Hư Linh Hiện Thân** 10s, toàn screen "vỡ", enemy tự chết khi qua mảnh vỡ. **Sau dùng skill này, mất 10% HP permanent for run** |

---

## 3.4 3★ Roster (15 characters — Material/Fodder)

3★ KHÔNG playable trong main run. Họ là **mặt người của thế giới** — dân thường vẫn cố sống sót. Khi player feed họ làm material, đây là **moral cost** game không tránh.

| # | Name | Background |
|---|---|---|
| 1 | **Lian Hàn Cung** | Cung thủ thuê chuyên săn beast Thanh Lâm Vực, làm cho ai trả nhiều nhất |
| 2 | **Garrick Đá Tay** | Đô vật giải nghệ làm bảo kê quán rượu Hoàng Sa Thành |
| 3 | **Yuna Linh Triệu** | Cô bé mồ côi 14 tuổi ở Sơ Lai Đài, gọi được vài linh thú nhỏ |
| 4 | **Brock Khiên Thép** | Cựu lính già 60 tuổi, không bỏ được thói quen cầm khiên |
| 5 | **Suki Hỏa Ngữ** | Học trò bỏ học của pháp sư lớn, vẫn còn vài chiêu dở dang |
| 6 | **Taro Lau Sậy** | Nông dân làng nhỏ, cầm giáo bảo vệ làng khi quái vật đến |
| 7 | **Mira Bước Nhẹ** | Trộm cắp đường phố Hoàng Sa Thành, nhanh nhẹn |
| 8 | **Doran Tử Mộc** | Bảo vệ rừng tự xưng, không có training chính thức |
| 9 | **Hana Cánh Hoa** | Vũ nữ địa phương biết múa kiếm như part of performance |
| 10 | **Jiro Sấm Vọng** | Người chăn cừu Bắc Thiên Cương, mang viên ngọc sét tổ tiên |
| 11 | **Elara Sương Mù** | Ảo thuật gia hội chợ, dùng thuật ảo che đậy thật giả |
| 12 | **Ronan Ngọn Lửa** | Cướp biển lưu vong từ Hồng Hải Vực, bị đồng đội phản bội |
| 13 | **Kira Bóng Nhanh** | Học trò sát thủ trẻ tuổi (15 tuổi), chưa hoàn thành training |
| 14 | **Dax Mạch Sắt** | Thợ rèn mạnh khỏe, biết tự vệ nhưng không phải warrior |
| 15 | **Liora Hỏa Tâm** | Cô bé bán đèn lồng, có ánh sáng kỳ lạ trong tay (chưa hiểu) |

---

# 4. COMBAT SYSTEM

## 4.1 Auto-Shooter Mechanics

**Core loop:**
- Player di chuyển bằng joystick — character auto-attack enemy gần nhất trong range
- Range tùy character (melee 1.8m, ranged 8-12m)
- Active skill manual trigger qua button (1-2 actives per character)
- Passive skill auto-trigger theo điều kiện

**Movement vs Facing tách biệt:**
- Movement: input direction
- Facing: target direction (nếu có target) hoặc movement direction (nếu không)
- Sử dụng `CharacterLocomotion.ExternalRotationControl = true` (xem CLAUDE.md Rule 4)

## 4.2 Skill Design Philosophy

**Mỗi skill phải có 4 layers** (xem `Docs/SKILL_DESIGN_GUIDE.md` chi tiết):

```
Anticipation → Execution → Impact → Resolution
   (windup)      (hit)     (reaction) (recovery)
```

**Skill = SO + Execution (Strategy Pattern):**
```csharp
public abstract class SkillDataSO : ScriptableObject {
    public abstract ISkillExecution CreateExecution(SkillContext ctx);
}
```

## 4.3 Combat Feel Layers (7 layers)

1. **Animation** — Visible motion
2. **Hitbox** — Physical impact
3. **Damage Number** — Numerical feedback
4. **Material Flash** — Color reaction
5. **Particle VFX** — World response
6. **Audio** — Sonic feedback
7. **Camera** — Screen-level reaction

(Chi tiết: `Docs/COMBAT_FEEL_GUIDE.md`)

---

# 5. PROGRESSION SYSTEMS

## 5.1 Star Ascension System

```
3★    →  Material only
4★ → +1 → +2 → +3 → +4 → +5  →  Eligible for 5★ ascension
5★ → +1 → +2 → +3 → +4 → +5  →  Eligible for 6★ Đỏ ascension
6★ Đỏ →  Final tier
```

### Cost Curve

| Level | Cost | Time-to-acquire (F2P) |
|---|---|---|
| 4★ → 4★+1 | 50 mảnh + 5 seal | ~3 ngày |
| 4★+1 → +2 | 100 mảnh + 10 seal | ~5 ngày |
| 4★+2 → +3 | 200 mảnh + 25 seal | ~10 ngày |
| 4★+3 → +4 | 400 mảnh + 50 seal | ~3 tuần |
| 4★+4 → +5 | 800 mảnh + 100 seal | ~6 tuần |
| 4★+5 → 5★ | **2000 mảnh + 1 Hỗn Độn Tinh Tủy** | ~3-6 tháng F2P |
| 5★ levels | similar pattern, higher cost | - |
| 5★+5 → 6★ Đỏ | **5000 mảnh + 5 Hỗn Độn + 3 dup 5★** | ~1+ năm F2P |

### Currency Sources

**Tinh Hồn Mảnh** (Star Soul Shard):
- Daily mission: 5-10/day base
- Boss kill: 10-30 per boss
- Banner shop: currency conversion
- 3★ feed: 1 con 3★ = 5 mảnh universal

## 5.2 Skill Unlock Ladder

Mỗi character có **4 skill slots:**
- **Auto-attack** (always available)
- **Active 1** (always available)
- **Active 2** (unlock at 4★+5)
- **Passive** (always available, upgrades through stars)

**Ascension không chỉ tăng số — mỗi cấp unlock NEW MECHANIC.**

## 5.3 Item / Thần Khí System

**6 slots per character:**

| Slot | Vietnamese | EN | Main stat |
|---|---|---|---|
| 1 | Vũ Khí | Weapon | ATK |
| 2 | Bùa Hộ Mệnh | Talisman | Utility |
| 3 | Áo Choàng | Robe | DEF |
| 4 | Giày | Boots | SPD |
| 5 | Linh Hồn Bảo Châu | Soul Pearl | Crit |
| 6 | Thiên Mệnh Ấn Khắc | Mandate Sigil | Unique passive |

**Rarity:**
- **Hạ Phẩm** (3★) — drop từ stage
- **Trung Phẩm** (4★) — drop boss + craft
- **Thượng Phẩm** (5★) — gacha + endgame

**Substats:** 1 main stat + 4 sub-stats. Sub-stats randomize khi farm. Re-roll bằng currency.

## 5.4 Set Bonus System

Mặc 2/4 món cùng set → activate bonus.

**Sample sets:**

| Set name | 2-piece | 4-piece |
|---|---|---|
| **Lôi Đình Phán Quyết** (Kai-friendly) | +20% lightning damage | Every kill spawns 1 lightning bolt |
| **Huyết Thi Đoàn Tụ** (Elyra-friendly) | Zombies +25% HP | Zombies merge auto when stack 5 |
| **Hư Không Bước** (Hiroshi-friendly) | +20% void damage | Teleport có afterimage tự attack |
| **Băng Tâm Vĩnh Cửu** (Hàn Tuyết-friendly) | +15% freeze chance | Frozen enemy → shatter explosion |
| **Cổ Long Linh Khí** (Drakon-friendly) | +20% fire damage | Burning enemy → spread to 2 nearby |

## 5.5 Lực Chiến — Combat Power System

**Pseudo-progression metric** displayed in profile.

```
Combat Power = (
    Character Base Stats × Star Multiplier
    + Skill Levels × 100
    + Sum of Item Stats
    + Set Bonus Bonus
    + Talent Tree Bonus
) × Player Account Level Multiplier
```

**Tier display:**

| Range | Title (VN) | Title (EN) |
|---|---|---|
| 0-1,000 | Sơ Cấp | Beginner |
| 1,000-5,000 | Trung Cấp | Intermediate |
| 5,000-20,000 | Cao Cấp | Advanced |
| 20,000-100,000 | Cường Giả | Elite |
| 100,000+ | Thiên Mệnh Đại Năng | Mandate Master |

**Combat Power KHÔNG quyết định win/loss trong run** (skill quyết định) — nhưng:
- Gating cho stages khó (cần >5,000 unlock Chapter 3)
- Leaderboard ranking
- Account profile title
- F2P vs Whale visible difference

---

# 6. GACHA & CURRENCY

## 6.1 Currency Matrix

| Currency | VN name | EN name | Earned via | Used for |
|---|---|---|---|---|
| 💎 Crystal | Linh Tinh Thạch | Astral Crystal | IAP, daily login, mission, achievement | Gacha pulls |
| 🪙 Gold | Vàng Tử Hà | Crimson Gold | Combat reward | Item enhancement |
| 🎫 Pull Ticket | Triệu Hồi Lệnh | Summon Token | Event, BP, mission | Alternative gacha pull |
| 🌙 Star Soul | Tinh Hồn Mảnh | Star Soul Shard | Duplicate convert, drop, shop | Star ascension |
| 🔮 Chaos Essence | Hỗn Độn Tinh Tủy | Chaos Essence | Boss-tier event, achievement | 5★/6★ ascension |
| ⭐ Soul Seal | Tinh Hồn Ấn | Soul Seal | Universal duplicate convert | Star ascension secondary |

## 6.2 Banner Types

### Tinh Tú Triệu Hồi (Standard Banner) — Permanent
- **Pool:** All non-event characters + items
- **Cost:** 160 Crystal/pull, 1600/x10
- **Rate:** 5★ — 0.6%, 4★ — 5.1%, 3★ — 94.3%
- **Soft pity:** từ pull 75 (rate +6% mỗi pull)
- **Hard pity:** pull 90 = guaranteed 5★

### Thần Vị Lâm Trần (Featured Character Banner)
- **Pool:** Limited 5★ featured + 4 rate-up 4★
- **Cost:** 160 Crystal/pull
- **50/50:** Khi pull 5★, 50% chance featured. Lost → next 5★ guaranteed featured
- **Pity carries over** giữa featured banner

### Sơ Tu Sơ Lai (Beginner Banner) — First 14 days
- **Cost:** 80 Crystal/pull (50% off)
- **Pity:** 10 pulls = guaranteed 4★+. 20 pulls = guaranteed 5★ (chọn từ pool 3 chars)
- **Max:** 30 pulls total

### Tinh Hồn Khế Ước (Item/Weapon Banner)
- **Pool:** 5★ Thượng Phẩm featured items
- **Pity:** 80 pulls

## 6.3 F2P Friendly Path

**Minimum F2P Crystal/month:**
- Daily login: 30 × 50 = 1,500
- Daily mission: 30 × 60 = 1,800
- Weekly mission: 4 × 200 = 800
- Achievement: ~500/month
- Battle Pass (free): ~1,200
- Events: 1,500-3,000

**Total: ~7,000-9,000 Crystal/month** = **43-56 pulls/month**

**F2P realistic:** 1 guaranteed featured 5★ every 2-3 months.

## 6.4 Gacha Animation Tier

- **3★ pull:** Cube blue glow, 0.2s
- **4★ pull:** Cube xanh, pulse glow, 0.5s
- **5★ pull:** Full cinematic — sky parts, silhouette descends, 2s reveal
- **5★ Featured + 50/50 win:** Same + golden aura bonus
- **6★ Đỏ pull (rare event banner):** Cinematic with red aura + name reveal

---

# 7. UI/UX VISION

## 7.1 Screen Hierarchy

```
MainMenu (Sơ Lai Đài Hub)
├── CharacterSelect
├── Gacha (locked in vertical slice)
├── Inventory (locked in vertical slice)
├── BattlePass (locked in vertical slice)
├── Shop (locked in vertical slice)
├── Settings
└── StageSelect
    ├── Chapter Map
    └── Endless Mode

InRun (Gameplay)
├── HUD
├── UpgradePanel (on level up)
├── PausePanel
└── GameOver/Victory Panel
```

## 7.2 Visual Layout — Main Menu

```
┌─────────────────────────────────────┐
│  [☰ Settings]      MYTHFALL         │
│                  SURVIVORS          │
│                                     │
│   [Featured Character Splash Art]   │
│                                     │
│   [Player Avatar]    Lv.42  ⚡ 8240 │
│                                     │
│   ┌───────────────────────┐         │
│   │      ▶ PLAY           │         │
│   └───────────────────────┘         │
│                                     │
│   [👥 Characters] [💰 Shop]         │
│   [🎰 Gacha]    [📜 Mission]        │
│   [🎒 Inventory] [🌟 BP]            │
│                                     │
│   💎 12,500   🪙 45,200   🎫 3      │
└─────────────────────────────────────┘
```

**In vertical slice:** Hide Gacha/Shop/Inventory/BP buttons hoặc show "Coming Soon" — KHÔNG hardcode hide, dùng feature flags để re-enable later.

## 7.3 Character Select Panel

```
┌─────────────────────────────────────┐
│  ← Back        Choose Hero          │
│                                     │
│   ┌──────────┐  ┌──────────┐        │
│   │  ★★★★    │  │  ★★★★    │        │
│   │  [Kai]   │  │  [Lyra]  │        │
│   │ Cô Lang  │  │Phản Đồ   │        │
│   │   ⚡ 1820│  │   🌙 1650│        │
│   └──────────┘  └──────────┘        │
│                                     │
│   ┌──────────┐  ┌──────────┐        │
│   │   ???    │  │   ???    │        │
│   │ 🔒 LOCK  │  │ 🔒 LOCK  │        │
│   │          │  │          │        │
│   └──────────┘  └──────────┘        │
│                                     │
│   [Selected: Kai]                   │
│   ┌───────────────────────┐         │
│   │   ▶ ENTER STAGE       │         │
│   └───────────────────────┘         │
└─────────────────────────────────────┘
```

**Star display logic:**
- Show actual star (4★ for vertical slice — both Kai/Lyra are 4★ Lv1)
- Locked characters show "???" + 🔒 — preview future content
- Hover/tap shows skill list summary

## 7.4 In-Run HUD

```
┌─────────────────────────────────────┐
│ ❤️ ████████░░ 850/1000   ⏱ 02:47    │
│ ⚡ ██████░░░░░░░░ Lv 4  120/250 XP  │
│                                     │
│                                     │
│         [Player + camera]           │
│                                     │
│                                     │
│                                     │
│                                     │
│ ┌───────┐                ┌─────────┐│
│ │ ◯     │                │  Skill  ││
│ │JOYSTK │                │ ⏱ 8s   ││
│ │       │                │   🌀    ││
│ └───────┘                └─────────┘│
└─────────────────────────────────────┘
```

**Top:** HP bar (red), XP bar (yellow), level indicator, run timer

**Bottom-left:** Virtual joystick (floating, appears on touch)

**Bottom-right:** Active skill button with cooldown ring

## 7.5 Upgrade Panel (on Level Up)

```
┌─────────────────────────────────────┐
│           LEVEL UP! ⚡              │
│        Choose an upgrade            │
│                                     │
│ ┌────────┐ ┌────────┐ ┌────────┐    │
│ │ COMMON │ │  RARE  │ │ COMMON │    │
│ │  [🗡]   │ │  [⚔]   │ │  [❤]   │    │
│ │Bloodthr│ │Volatile│ │Iron Sk │    │
│ │+12% ATK│ │20% AoE │ │+15% HP │    │
│ └────────┘ └────────┘ └────────┘    │
│                                     │
│       [🎲 Reroll (1 free)]          │
└─────────────────────────────────────┘
```

**Border colors:** Common (gray), Rare (blue), Epic (purple), Game-Changer (gold)

## 7.6 Game Over / Victory Panel

```
┌─────────────────────────────────────┐
│                                     │
│         💀 YOU FELL                 │
│      "Hư Vô đã thắng..."            │
│                                     │
│   ⏱ Time Survived:   04:32         │
│   💀 Wave Reached:   8              │
│   ⚔ Enemies Slain:  327            │
│   📈 Level Reached:  6              │
│                                     │
│   ┌──────────────┐                  │
│   │  🔄 Retry    │                  │
│   └──────────────┘                  │
│   ┌──────────────┐                  │
│   │  🏠 Hub      │                  │
│   └──────────────┘                  │
└─────────────────────────────────────┘
```

## 7.7 Inventory Panel (post-vertical-slice)

```
┌─────────────────────────────────────┐
│  ← Back          Inventory          │
│                                     │
│  Tabs: [Characters] [Items] [Mat]   │
│                                     │
│  ┌──────────────┬───────────────┐   │
│  │ Filter       │ Grid View     │   │
│  │ ─────────    │               │   │
│  │ ★★★ ★★★★    │  ▢ ▢ ▢ ▢ ▢ ▢  │   │
│  │ ★★★★★ ★★★★★★│  ▢ ▢ ▢ ▢ ▢ ▢  │   │
│  │              │  ▢ ▢ ▢ ▢ ▢ ▢  │   │
│  │ Class:       │  ▢ ▢ ▢ ▢ ▢ ▢  │   │
│  │ ☑ Warrior    │               │   │
│  │ ☑ Archer     │  Selected:    │   │
│  │ ☐ Mage       │  Kai ★★★★    │   │
│  │              │  Lv 25/40     │   │
│  └──────────────┴───────────────┘   │
│                                     │
│  [Ascend]  [Equip]  [Skill Up]      │
└─────────────────────────────────────┘
```

## 7.8 Gacha Banner Screen (post-vertical-slice)

```
┌─────────────────────────────────────┐
│  ← Back     [Featured Character]    │
│                                     │
│        [Big Splash Art]             │
│                                     │
│       「LYRA — KẺ PHẢN ĐỒ」         │
│        ★★★★★ Featured                │
│                                     │
│  Banner ends in: 7d 14h 22m         │
│  Pity: 47/90                        │
│                                     │
│  ┌──────────────┐ ┌──────────────┐  │
│  │ x1 PULL      │ │ x10 PULL     │  │
│  │ 💎 160       │ │ 💎 1,600     │  │
│  └──────────────┘ └──────────────┘  │
│                                     │
│  [📜 View Rates]  [📋 Pull History] │
└─────────────────────────────────────┘
```

---

# 8. VISUAL DIRECTION

## 8.1 Art Style Statement

> **Toon Stylized 3D với cel-shaded edge** — Genshin Impact (character fidelity) + Honkai Star Rail (UI polish) + League of Legends Wild Rift (top-down readability) + Hollow Knight (atmospheric melancholy)

## 8.2 Character Design Rules

- **Tỉ lệ:** 7-head tall (slightly stylized, không chibi)
- **Silhouette:** clear từ top-down 50° camera
- **Outline:** 1.5-2px black inverse hull shader
- **Face:** lớn hơn realistic 10-15%, detail ở mắt
- **Clothing:** bold color blocks, minimal small detail (compression friendly)

## 8.3 Color Palette per Region

| Region | Primary | Secondary | Accent |
|---|---|---|---|
| Bắc Thiên Cương | #5C7AB8 (cold blue) | #FFFFFF (white) | #FFD700 (lightning gold) |
| Thanh Lâm Vực | #5DCAA5 (forest green) | #97C459 (light green) | #F4C0D1 (flower pink) |
| Hỏa Diễm Quốc | #D85A30 (ember orange) | #993C1D (dark red) | #FFD700 (flame gold) |
| Hàn Băng Đảo | #B5D4F4 (ice blue) | #E6F1FB (frost white) | #378ADD (deep blue) |
| Hoàng Sa Thành | #EF9F27 (sand gold) | #BA7517 (dark gold) | #4A1B0C (shadow brown) |
| U Minh Cốc | #3C3489 (deep purple) | #26215C (dark purple) | #7F77DD (mystic purple) |
| Cửu Thiên Sơn | #AFA9EC (sky purple) | #F1EFE8 (cloud white) | #5DCAA5 (mountain green) |
| Hồng Hải Vực | #993556 (crimson) | #D4537E (rose) | #042C53 (deep navy) |
| Hư Linh Cảnh | #2C2C2A (void black) | #5F5E5A (ash gray) | #534AB7 (void purple) |

## 8.4 Shader Pipeline

**Base = VAT_Toonlit.shader** đã có. Extend thêm:

1. **Cel shading:** 3-tone ramp (shadow / mid / highlight)
2. **Rim light:** Fresnel edge, màu tím/xanh
3. **Outline:** Inverse hull, 0.02 thickness, đen
4. **Emission:** Hit flash override (red 0.3s)
5. **Vertex AO:** Multiply diffuse với Mesh.colors.r (terrain)
6. **VAT animation:** Sample position texture cho enemy vertex anim

## 8.5 VFX Language

- **Hit spark:** 8 particle burst, yellow/white, 0.1s
- **Death burst:** Larger, purple/blue, scale by enemy size
- **Level up ring:** Shockwave quad scale-out + fade
- **Elemental VFX:** Match element color
  - 🔥 Fire: orange triangular flame
  - ❄️ Ice: blue hexagonal shard
  - ⚡ Lightning: white jagged line
  - ☠️ Poison: green bubble cloud
  - 🌀 Void: purple distortion ring

**ALL VFX pooled:**
```csharp
Bill.Pool.Register("VFX_HitSpark", prefab, 50);
Bill.Pool.Spawn("VFX_HitSpark", hitPos);
```

## 8.6 Environment Theme — Chapter 1 (Vertical Slice)

**Sundered Grove (Thanh Lâm Vực corrupted):**

- **Trees:** stylized lowpoly, 3 variants — bark có vết Hư Vô xám
- **Ground:** moss + corrupted vegetation
- **Props:** mushroom phát quang xanh, fallen log, glowing fungi (lights)
- **Lighting:** cool green ambient + fog xanh nhạt
- **Skybox:** twilight purple-green, không có mặt trời rõ
- **Atmosphere:** sương mù xanh lục bệnh hoạn, particle nhỏ float

---

# 9. AUDIO DIRECTION

## 9.1 BGM per State

| State | Track | Style | Duration |
|---|---|---|---|
| Main Menu | bgm_menu | Calm orchestral + acoustic | Loop 2min |
| Character Select | bgm_hub | Soft pad + bell | Loop 1.5min |
| Combat (Ch 1) | bgm_combat_ch1 | Celtic folk + percussion | Loop 3min |
| Boss Fight | bgm_boss | Choir + brass + percussion | Loop 2min |
| Victory | sfx_victory_fanfare | Trumpets, bells | 5s |
| Defeat | sfx_defeat | Sad strings | 3s |

## 9.2 Voice Direction per Character

| Character | Voice profile | Key barks |
|---|---|---|
| Kai | Low, gruff, Northern accent | Battle cries thay câu nói dài |
| Lyra | Soft, precise, melodic | Sing-songy khi cast |
| Akari | High-energy + Yamabuki banter | "Hey, đừng có giật dây tao!" (Yamabuki) |
| Liên | Câm lặng, words như haiku | Chỉ tiếng thở + hiếm câu 3-5 từ |
| Aurelia | Quý tộc, ngôn từ chuẩn | "Rừng nhớ — và rừng phán xét" |
| Marcus | Trầm, nặng | "Đứng giữa..." (whisper) |
| Soren | Sarcastic, hay đùa | "Dao hay thanh kiếm? Tao ăn cả hai" |
| Kestrel | Lạnh + child-like | "Tôi nên cảm thấy gì lúc này?" |
| Torin | Drunk-philosophical | "Sét yếu... vì tôi yếu" |
| Mei | Pragmatic, đếm số | "Đó là 12 vàng. Tiếp tục" |

## 9.3 SFX Categories

**Combat (per character + universal):**
- Melee swing whoosh + impact thud
- Arrow/projectile release + impact
- Magic charge whir + release boom
- Explosion small/big
- Enemy hurt + death

**Progression:**
- XP pickup chime (per gem size)
- Level up ascending chime
- Gacha pull (3-tier: standard, rare, legendary)

**UI:**
- Button click (subtle)
- Panel open/close whoosh
- Notification ping
- Coin drop (currency feedback)
- Error buzz

## 9.4 Audio Architecture

```csharp
// Channels
Bill.Audio.SetVolume(AudioChannel.Master, 1.0f);
Bill.Audio.SetVolume(AudioChannel.Music, 0.6f);  // BGM lower than SFX
Bill.Audio.SetVolume(AudioChannel.SFX, 1.0f);
Bill.Audio.SetVolume(AudioChannel.UI, 0.8f);
Bill.Audio.SetVolume(AudioChannel.Voice, 1.0f);

// Mobile concurrent limit
Max 8 AudioSources active
Priority: player > enemy > ambient
Distance attenuation cho 3D
```

---

# 10. LOCALIZATION PLAN

## 10.1 Languages Roadmap

| Lang | Code | Priority | Audience | Implementation |
|---|---|---|---|---|
| **Vietnamese** | vi | P0 | Home market, soft launch | Sprint 1 (default) |
| **English** | en | P0 | Global launch | Sprint 1 |
| **Chinese Simplified** | zh-CN | P1 | Largest gacha market | Post-launch month 1 |
| **Japanese** | ja | P1 | Highest RPD | Post-launch month 1 |
| **Korean** | ko | P2 | High spend market | Post-launch month 2 |
| **Spanish** | es | P2 | Latin America | Post-launch month 3 |

## 10.2 Architecture Approach

**Custom JSON-based system** (chọn vì lightweight + dễ control hơn Unity Localization Package).

**Key conventions:**
```
ui.menu.play              → "Chơi" / "Play"
ui.menu.gacha             → "Triệu Hồi" / "Summon"
character.kai.name        → "Kai Lôi Phong" / "Kai Stormbringer"
character.kai.title       → "Cô Lang" / "The Lone Wolf"
character.kai.lore        → "Hậu duệ Lôi Hoàng..." / "Descendant of..."
skill.berserker_rush.name → "Lôi Bộc" / "Berserker Rush"
skill.berserker_rush.desc → "Lao về phía trước..." / "Charge forward..."
```

**File structure:**
```
Resources/Localization/
├── lang_vi.json    (Vietnamese, primary)
├── lang_en.json    (English)
├── lang_zh.json    (future)
└── lang_ja.json    (future)
```

**Data-driven:**
- ScriptableObject (CharacterDataSO, SkillDataSO) chứa **localization keys** thay vì raw strings
- UI text dùng `LocalizedText` component tự động bind localization
- Runtime switch language → toàn UI auto-update qua event

(Chi tiết: `Docs/LOCALIZATION_GUIDE.md`)

## 10.3 Critical Rules

1. **TUYỆT ĐỐI KHÔNG hardcode user-facing string trong code**
2. **Mọi UI text phải qua LocalizationService**
3. **CharacterDataSO/SkillDataSO chứa localization key, không phải raw text**
4. **Font support CJK + Vietnamese từ ngày 1**
5. **UI layout flexible** — không fix width text containers

---

# 11. VERTICAL SLICE SCOPE

## 11.1 What's IN (2 weeks)

✅ **Implementation:**
- 2 characters (Kai 4★, Lyra 4★ — both Lv1, no ascension UI yet)
- 3 enemy types (Swarmer, Brute, Shooter) + 1 boss (Rotwood)
- Auto-attack + Active skill + Passive skill mỗi character
- Wave spawner basic
- In-run XP + Level up + 8 upgrade cards
- HUD (HP, XP, skill button, timer)
- Polish layer 1: hitstop, screen shake, damage numbers, material flash, knockback
- VFX placeholders + audio basic
- Localization VN + EN
- Save/load character choice

✅ **Architecture (data-only, UI placeholder):**
- CharacterDataSO supports star rating field (display only)
- Skill ascension table in SO (data ready, UI shows current tier only)
- Localization service ready for adding languages
- Pool system ready for VFX/enemy variety

## 11.2 What's OUT (deferred to post-slice)

❌ **Not implemented:**
- Gacha system (UI button shows "Coming Soon" / locked)
- Inventory + equipment system
- Star ascension mechanics (UI display "max 4★+1" placeholder)
- Battle Pass
- IAP monetization
- Multi-chapter (only Chapter 1 area)
- Cloud save / backend
- Multi-language beyond VN/EN
- 3★ characters (skip entirely)
- 5★ characters (skip entirely — only 2 4★ to test)
- Item drop / set bonus

## 11.3 Architecture Foundation for Future

**Built to expand without rewrites:**
- `CharacterDataSO` schema includes star rating + skill list (full progression)
- `SkillDataSO` schema includes tier-based unlocks (4★ → 5★ → 6★)
- `InventoryService` schema includes ownedCharacterIds + currency (just empty in slice)
- `LocalizationService` ready for any number of languages
- `Bill.Pool` registered cho all categories (VFX, enemy, projectile, gem)
- Scene flow + state machine support adding new states (Gacha, Inventory, etc.)

---

# 12. POST-SLICE ROADMAP

## Month 2-3: Core Meta Systems
- Gacha system implementation
- Inventory + equipment basic
- Star ascension UI + mechanics
- 5 thêm 4★ characters (total 7)
- 3 thêm 5★ characters (total 3)

## Month 4-5: Full Content
- All 10 4★ + 7 5★ characters
- Set bonus system
- 3★ fodder system
- Chapter 2 + 3 maps
- Multi-language: zh-CN, ja added

## Month 6-7: Monetization Live
- IAP integration
- Battle Pass
- Daily mission + weekly mission
- Soft launch VN/PH
- Firebase backend live

## Month 8-9: Global Launch
- All chapters polish
- Korean + Spanish localization
- Full marketing push
- Live ops content cadence

---

## APPENDIX A: KEY DECISIONS LOG

- **Why 9 lục địa?** — 9 = perfect for expansion roster (1 char per region average) + Chinese cultural significance
- **Why Vietnamese names primary?** — Solo dev VN, soft launch VN first, easier voice direction
- **Why "Anchor" not "Sanctuary"?** — Anchor implies physical hold against tide, Sanctuary too passive
- **Why 4★ playable not 3★?** — Reduce inventory clutter + give starter character meaningful power feeling
- **Why Hư Vô abstract not "Demon Lord"?** — Mature antagonist, allows mystery + thematic depth (memory loss)

## APPENDIX B: GLOSSARY (VN ↔ EN)

| Vietnamese | English | Note |
|---|---|---|
| Cửu Châu Thiên Cổ | Nine Realms of Ancient Sky | World name |
| Thiên Mạc | The Veil | Boundary between realms |
| Thiên Kiếp Đại Lạc | The Sundering | Catastrophic event 327 years ago |
| Thất Dạ Phá Thiên | Seven Nights of Sky Breaking | Specific 7-day duration |
| Hư Vô | The Void Hollow | Antagonist force |
| Hư Linh Quỷ | Voidborn | Corrupted enemies |
| Thánh Hỏa Đài | The Anchors | Defensive strongholds |
| Sơ Lai Đài | The First Hearth | Player home Anchor |
| Thiên Mệnh Ấn | Mandate Seal | Player artifact |
| Thiên Mệnh Chi Chủ | Mandate Holder | Player title |
| Lực Chiến | Combat Power | Stat metric |
| Tinh Hồn Mảnh | Star Soul Shard | Ascension material |
| Hỗn Độn Tinh Tủy | Chaos Essence | Premium ascension material |

---

*End of GAME_DESIGN.md — Mythfall: Survivors v1.0*
*~700 lines, comprehensive vision document for full game development*
