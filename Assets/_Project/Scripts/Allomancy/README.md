# Allomancy System

*Last updated: March 21, 2026*

---

## Metal Types

### Basic Metals (16)

| Metal | Push/Pull | Effect |
|-------|-----------|--------|
| Steel | Push | Push metals away |
| Iron | Pull | Pull metals toward |
| Tin | Push | Enhanced senses |
| Pewter | Push | Enhanced strength/speed |
| Zinc | Push | Riot emotions (enrage) |
| Brass | Pull | Soothe emotions (calm) |
| Copper | Push | Hide Allomantic pulses |
| Bronze | Pull | Detect Allomancers |
| Atium | Push | See enemy futures |
| Gold | Push | See past self |
| Electrum | Push | See own future |
| Aluminum | Push | Purge all metals |
| Duralumin | Push | Burst power |
| Bendalloy | Push | Speed up time |
| Cadmium | Push | Slow down time |
| Chromium | Push | Wipe enemy metals |
| Nicrosil | Push | Amplify nearby metals |

### God Metals (3)

| Metal | Effect |
|-------|--------|
| Atium | See futures |
| Malatium | Reveal true nature |
| Lerasium | Mistborn power |

---

## Script Structure

Each metal ability follows this pattern:

```csharp
public class MetalName : MonoBehaviour
{
    // SETTINGS
    public float metalCostPerSecond = 2f;
    
    // STATE
    private bool isBurning = false;
    
    // EVENTS
    public System.Action OnBurnStart;
    public System.Action OnBurnEnd;
    
    // PUBLIC API
    public bool IsBurning => isBurning;
    
    void Update()
    {
        // Input handling
        // Effect application
        // Metal drainage
    }
    
    void DrainMetal()
    {
        MetalReserveManager metals = GetComponent<MetalReserveManager>();
        if (metals.UseMetal(MetalType.X, amount))
        {
            // Success
        }
        else
        {
            // Not enough metal
        }
    }
}
```

---

## Input Bindings

| Metal | Key |
|-------|-----|
| Steel Push | Right Mouse |
| Iron Pull | Left Mouse |
| Tin | E |
| Pewter | Q |
| Zinc | Z |
| Brass | X |
| Copper | C |
| Bronze | V |
| Atium | T |
| Gold | G |
| Electrum | 5 |
| Aluminum | O |
| Duralumin | R |
| Bendalloy | 8 |
| Cadmium | 9 |

---

## Integration

### Required Components

Add these to the player GameObject:

1. `MetalReserveManager` - Manages metal reserves
2. Individual metal scripts as needed

### Example Setup

```
Player
├── MetalReserveManager.cs
├── SteelPush.cs
├── IronPull.cs
├── PewterBurn.cs
├── TinEnhance.cs
└── ...
```

---

## Metal Costs

| Metal | Cost/Second | Total Duration |
|-------|-------------|----------------|
| Steel | 2 | 50 sec |
| Iron | 2 | 50 sec |
| Tin | 2 | 50 sec |
| Pewter | 3 | 33 sec |
| Zinc | 3 | 33 sec |
| Brass | 3 | 33 sec |
| Copper | 4 | 25 sec |
| Bronze | 2 | 50 sec |
| Atium | 10 | 10 sec |
| Gold | 8 | 12 sec |
