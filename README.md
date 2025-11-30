# SpaceScrappers

A Unity game project.

## Development

This project is built with Unity. Open the project folder in Unity to get started.

## Requirements

- Unity (check ProjectSettings/ProjectVersion.txt for the specific version)

## Testing Co-op / Multiplayer

This project includes tools to make testing co-op gameplay easier. Here are three methods:

### Method 1: ParrelSync (Recommended for Editor Testing)

**ParrelSync** allows you to run multiple Unity Editor instances with synchronized projects. This is the fastest way to test co-op during development.

**Setup:**

1. After opening the project, Unity will automatically install ParrelSync from the package manifest
2. Once installed, you'll see a "ParrelSync" menu in the Unity Editor
3. Go to `ParrelSync > Clones Manager` to open the clones window
4. Click "Create New Clone" to create a synchronized copy of your project
5. Open the clone in a new Unity Editor instance

**Usage:**

- **Main Editor**: Your primary development instance (make changes here)
- **Clone Editor**: Automatically syncs with the main editor (use for testing)
- Changes in the main editor sync to clones automatically
- Each instance can run Play Mode independently
- Perfect for testing networking, input handling, and multiplayer interactions

**Tips:**

- Use different screen positions or window sizes to distinguish instances
- Each clone uses a separate project folder, so they won't conflict
- Clones are automatically excluded from version control

### Method 2: Quick Build Tool

For testing with actual builds (more realistic but slower iteration):

1. Go to `Tools > Quick Build Co-op` in the Unity Editor
2. Configure:
   - **Build Path**: Where builds will be saved (default: `Builds/`)
   - **Number of Builds**: How many instances to build (default: 2)
   - **Development Build**: Enable for debugging
   - **Auto Run**: Automatically launch the first build after completion
3. Click "Build All" to create multiple builds
4. Run each build executable simultaneously for co-op testing

**Advantages:**

- Tests the actual build configuration
- More realistic performance testing
- Can test on different machines over network

**Disadvantages:**

- Slower iteration (must rebuild after changes)
- Takes more disk space

### Method 3: Unity Multiplayer Play Mode (If using Netcode for GameObjects)

If you're using Unity Netcode for GameObjects, you can use the built-in Multiplayer Play Mode:

1. Install Netcode for GameObjects package
2. In the Unity Editor, you'll see multiplayer play mode options
3. Configure player count and roles
4. Press Play to run multiple simulated players in one editor instance

**Note:** This method requires Netcode for GameObjects to be set up in your project.

## Recommended Workflow

1. **During Active Development**: Use **ParrelSync** for fastest iteration
2. **Before Committing**: Use **Quick Build** to test actual builds
3. **For Network Testing**: Use builds on separate machines or VMs

## Troubleshooting

**ParrelSync not appearing:**

- Wait for Unity to finish importing packages
- Check `Window > Package Manager` to verify ParrelSync is installed
- Restart Unity Editor

**Build conflicts:**

- Each build instance uses separate folders, so they shouldn't conflict
- If using localhost networking, ensure each instance uses different ports
- Check your networking code handles multiple local instances correctly
