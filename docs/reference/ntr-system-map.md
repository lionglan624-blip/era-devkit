# NTR System Map

Reference for existing NTR system implementation locations.

---

## File Structure

```
Game/ERB/NTR/
├── NTR.ERB              # Main, visitor appearance, takeout
├── NTR_FRIENDSHIP.ERB   # Mood progression, privacy level, locking
├── NTR_VISITOR.ERB      # Visitor movement AI
├── NTR_SEX.ERB          # Sexual activity processing
└── NTR_TAKEOUT.ERB      # Takeout processing
```

---

## Function Reference by Feature

### Privacy Level System

| Item | Value |
|------|-----|
| **Function** | `GET_ROOM_SECURITY(location)` |
| **File** | NTR_FRIENDSHIP.ERB:154 |
| **Return Value** | 0=Open, 1=Normal, 2=Private Room, 3=Personal Room |

### Mood Limit Judgment

| Item | Value |
|------|-----|
| **Function** | `JUDGE_VISITOR_MOOD_MAX(slave, privacy_level, num_present)` |
| **File** | NTR_FRIENDSHIP.ERB:184 |

### Locking System

| Function | Description |
|----------|-------------|
| `tryLock(character, location)` | Normal lock |
| `tryLockBolt(character, location)` | Bolt lock (difficult to unlock) |
| `isLocked(location)` | Check locked state |

### Visitor Movement AI

| Function | Description |
|----------|-------------|
| `JUDGE_VISITOR_MOVE_POS` | Visitor movement judgment |

**Related Variables**:
- `FLAG:訪問者のお気に入り` (visitor's favorite, 999 = not set)
- `FLAG:訪問者の嫌いな相手` (visitor's disliked person)

### Other Features

| Feature | Function/Variable |
|---------|-------------------|
| Takeout | `VISITER_TAKINGOUT` → `場所_訪問者宅` (location 900) |
| Sleep assault | `FLAG:睡姦フラグ` (sleep assault flag) |
| Cheating approval | `TALENT:浮気公認` (cheating approval, 0-4 levels) |
| Witness check | `NTR_CHK_VISIBLE` |
| Peeping discovery | `CFLAG:覗き発覚回数` (peeping discovery count) |

---

## Links

- [netorase-system.md](../designs/netorase-system.md)
