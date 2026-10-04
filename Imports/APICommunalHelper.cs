using Microsoft.Xna.Framework.Graphics;
using MonoMod.ModInterop;

namespace ChroniaHelper.Imports;

#region DashStates

[ModImportName("CommunalHelper.DashStates")]
public static class APICommunalHelperDashStates
{
    #region DreamTunnel

    /// <returns> The player state id of the DreamTunnelDash. </returns>
    public static Func<int> GetDreamTunnelDashState;

    /// <returns> Whether the player currently has a DreamTunnel dash available. </returns>
    public static Func<bool> HasDreamTunnelDash;

    /// <returns> The amount of DreamTunnel dashes the player currently has. </returns>
    public static Func<int> GetDreamTunnelDashCount;

    /// <summary> Sets the amount of DreamTunnel dashes the player currently has. </summary>
    public static Action<int> SetDreamTunnelDashCount;

    /// <returns> Whether the player is currently attacking with a DreamTunnel dash. </returns>
    public static Func<bool> IsDreamTunnelDashAttacking;

    /// <summary>
    /// Creates a component which runs the given callbacks when the player enters / exits a DreamTunnel.
    /// </summary>
    public static Component dreamTunnelInteraction(this Action<Player> onPlayerEnter, Action<Player> onPlayerExit)
    {
        return DreamTunnelInteraction?.Invoke(onPlayerEnter, onPlayerExit);
    }
    public delegate Component DreamTunnelInteractionDelegate(Action<Player> onPlayerEnter, Action<Player> onPlayerExit);
    public static DreamTunnelInteractionDelegate DreamTunnelInteraction;

    #endregion

    #region Seeker

    /// <returns> Whether the player currently has a Seeker dash available. </returns>
    public static Func<bool> HasSeekerDash;

    /// <summary> Sets whether the player currently has a Seeker dash available. </summary>
    public static Action<bool> SetSeekerDash;

    /// <returns> Whether the player is currently attacking with a Seeker dash. </returns>
    public static Func<bool> IsSeekerDashAttacking;

    #endregion
}

#endregion

#region Elytra

[ModImportName("CommunalHelper.Elytra")]
public static class APICommunalHelperElytra
{
    #region Elytra

    /// <returns> The player state id of the Elytra glide. </returns>
    public static Func<int> GetElytraState;

    /// <returns> Whether the player is allowed to deploy the Elytra in the current session. </returns>
    public static Func<bool> IsElytraEnabled;

    /// <summary> Sets whether the player is allowed to deploy the Elytra in the current session. </summary>
    public static Action<bool> SetElytraEnabled;

    /// <returns> Whether the given player has infinite Elytra. </returns>
    public static bool hasInfiniteElytra(this Player player) => HasInfiniteElytra?.Invoke(player) ?? false;
    public static Func<Player, bool> HasInfiniteElytra;

    /// <summary> Sets whether the given player has infinite Elytra. </summary>
    public static void setInfiniteElytra(this Player player, bool enabled)
    {
        SetInfiniteElytra?.Invoke(player, enabled);
    }
    public static Action<Player, bool> SetInfiniteElytra;

    /// <summary>
    /// Creates a component which runs the given callback whenever the player collides with an Elytra.
    /// The callback returns an ElytraCollision.Result describing the resulting behavior.
    /// </summary>
    public static Component elytraCollision(this Func<Player, int> onCollide)
    {
        return ElytraCollision?.Invoke(onCollide);
    }
    public delegate Component ElytraCollisionDelegate(Func<Player, int> onCollide);
    public static ElytraCollisionDelegate ElytraCollision;

    #endregion
}

#endregion

#region Entities

[ModImportName("CommunalHelper.Entities")]
public static class APICommunalHelperEntities
{
    #region Misc

    #region Melvin

    /// <summary>
    /// Creates a component which makes the attached entity targetable by Melvin entities, with the given priority.
    /// </summary>
    public static Component melvinTargetable(this int priority)
    {
        return MelvinTargetable?.Invoke(priority);
    }
    public static Func<int, Component> MelvinTargetable;

    #endregion

    #endregion
}

#endregion

#region Shape3DExports

[ModImportName("CommunalHelper.Shape3DExports")]
public static class APICommunalHelperShape3DExports
{
    #region Shape3D

    /// <summary>
    /// Creates a Shape3D component from the given mesh, which may be a Mesh<VertexPCTN> or an
    /// IEnumerable<Tuple<Mesh<VertexPCTN>, Texture2D>>.
    /// </summary>
    public static Component createShape3D(this object mesh)
    {
        return CreateShape3D?.Invoke(mesh);
    }
    public static Func<object, Component> CreateShape3D;

    /// <returns> The RainbowMix of the given Shape3D component. </returns>
    public static float getRainbowMix(this Component shape3d)
    {
        return GetRainbowMix?.Invoke(shape3d) ?? 0f;
    }
    public static Func<Component, float> GetRainbowMix;

    /// <summary> Sets the RainbowMix of the given Shape3D component. </summary>
    public static void setRainbowMix(this Component shape3d, float value)
    {
        SetRainbowMix?.Invoke(shape3d, value);
    }
    public static Action<Component, float> SetRainbowMix;

    /// <returns> The texture of the given Shape3D component. </returns>
    public static Texture2D getTexture(this Component shape3d)
    {
        return GetTexture?.Invoke(shape3d);
    }
    public static Func<Component, Texture2D> GetTexture;

    /// <summary> Sets the texture of the given Shape3D component. </summary>
    public static void setTexture(this Component shape3d, Texture2D texture)
    {
        SetTexture?.Invoke(shape3d, texture);
    }
    public static Action<Component, Texture2D> SetTexture;

    /// <returns> The render depth of the given Shape3D component. </returns>
    public static int getDepth(this Component shape3d)
    {
        return GetDepth?.Invoke(shape3d) ?? 0;
    }
    public static Func<Component, int> GetDepth;

    /// <summary> Sets the render depth of the given Shape3D component. </summary>
    public static void setDepth(this Component shape3d, int value)
    {
        SetDepth?.Invoke(shape3d, value);
    }
    public static Action<Component, int> SetDepth;

    /// <returns> The tint of the given Shape3D component, as a Vector4. </returns>
    public static Vector4 getTint(this Component shape3d)
    {
        return GetTint?.Invoke(shape3d) ?? default;
    }
    public static Func<Component, Vector4> GetTint;

    /// <summary> Sets the tint of the given Shape3D component, as a Vector4. </summary>
    public static void setTint(this Component shape3d, Vector4 tint)
    {
        SetTint?.Invoke(shape3d, tint);
    }
    public static Action<Component, Vector4> SetTint;

    /// <summary> Sets the tint of the given Shape3D component, as a Color. </summary>
    public static void setTint_Color(this Component shape3d, Color tint)
    {
        SetTint_Color?.Invoke(shape3d, tint);
    }
    public static Action<Component, Color> SetTint_Color;

    /// <returns> The world position of the given Shape3D component. </returns>
    public static Vector3 getPosition(this Component shape3d)
    {
        return GetPosition?.Invoke(shape3d) ?? default;
    }
    public static Func<Component, Vector3> GetPosition;

    /// <summary> Sets the world position of the given Shape3D component. </summary>
    public static void setPosition(this Component shape3d, Vector3 pos)
    {
        SetPosition?.Invoke(shape3d, pos);
    }
    public static Action<Component, Vector3> SetPosition;

    /// <returns> The highlight strength of the given Shape3D component. </returns>
    public static float getHighlightStrength(this Component shape3d)
    {
        return GetHighlightStrength?.Invoke(shape3d) ?? 0f;
    }
    public static Func<Component, float> GetHighlightStrength;

    /// <summary> Sets the highlight strength of the given Shape3D component. </summary>
    public static void setHighlightStrength(this Component shape3d, float value)
    {
        SetHighlightStrength?.Invoke(shape3d, value);
    }
    public static Action<Component, float> SetHighlightStrength;

    /// <returns> The depth edge strength of the given Shape3D component. </returns>
    public static float getDepthEdgeStrength(this Component shape3d)
    {
        return GetDepthEdgeStrength?.Invoke(shape3d) ?? 0f;
    }
    public static Func<Component, float> GetDepthEdgeStrength;

    /// <summary> Sets the depth edge strength of the given Shape3D component. </summary>
    public static void setDepthEdgeStrength(this Component shape3d, float value)
    {
        SetDepthEdgeStrength?.Invoke(shape3d, value);
    }
    public static Action<Component, float> SetDepthEdgeStrength;

    /// <returns> The normal edge strength of the given Shape3D component. </returns>
    public static float getNormalEdgeStrength(this Component shape3d)
    {
        return GetNormalEdgeStrength?.Invoke(shape3d) ?? 0f;
    }
    public static Func<Component, float> GetNormalEdgeStrength;

    /// <summary> Sets the normal edge strength of the given Shape3D component. </summary>
    public static void setNormalEdgeStrength(this Component shape3d, float value)
    {
        SetNormalEdgeStrength?.Invoke(shape3d, value);
    }
    public static Action<Component, float> SetNormalEdgeStrength;

    /// <returns> The highlight upper bound of the given Shape3D component. </returns>
    public static float getHighlightUpperBound(this Component shape3d)
    {
        return GetHighlightUpperBound?.Invoke(shape3d) ?? 0f;
    }
    public static Func<Component, float> GetHighlightUpperBound;

    /// <summary> Sets the highlight upper bound of the given Shape3D component. </summary>
    public static void setHighlightUpperBound(this Component shape3d, float value)
    {
        SetHighlightUpperBound?.Invoke(shape3d, value);
    }
    public static Action<Component, float> SetHighlightUpperBound;

    /// <returns> The highlight lower bound of the given Shape3D component. </returns>
    public static float getHighlightLowerBound(this Component shape3d)
    {
        return GetHighlightLowerBound?.Invoke(shape3d) ?? 0f;
    }
    public static Func<Component, float> GetHighlightLowerBound;

    /// <summary> Sets the highlight lower bound of the given Shape3D component. </summary>
    public static void setHighlightLowerBound(this Component shape3d, float value)
    {
        SetHighlightLowerBound?.Invoke(shape3d, value);
    }
    public static Action<Component, float> SetHighlightLowerBound;

    /// <returns> The transform matrix of the given Shape3D component. </returns>
    public static Matrix getMatrix(this Component shape3d)
    {
        return GetMatrix?.Invoke(shape3d) ?? default;
    }
    public static Func<Component, Matrix> GetMatrix;

    /// <summary> Sets the transform matrix of the given Shape3D component. </summary>
    public static void setMatrix(this Component shape3d, Matrix matrix)
    {
        SetMatrix?.Invoke(shape3d, matrix);
    }
    public static Action<Component, Matrix> SetMatrix;

    #endregion

    #region Shapes

    /// <summary> Creates a box mesh of the given size, using a single texture on all sides. </summary>
    public static object createBox(float sx, float sy, float sz, MTexture texture)
    {
        return CreateBox?.Invoke(sx, sy, sz, texture);
    }
    public static Func<float, float, float, MTexture, object> CreateBox;

    /// <summary> Creates a box mesh of the given size, using a single texture on all sides, offset and tinted. </summary>
    public static object createBox_Offset_Color(Vector3 offset, float sx, float sy, float sz, MTexture texture, Color tint)
    {
        return CreateBox_Offset_Color?.Invoke(offset, sx, sy, sz, texture, tint);
    }
    public static Func<Vector3, float, float, float, MTexture, Color, object> CreateBox_Offset_Color;

    /// <summary> Creates a box mesh of the given size, using a separate texture for each side. </summary>
    public static object createBox_SideTextures(float sx, float sy, float sz, MTexture back, MTexture front, MTexture left, MTexture right, MTexture top, MTexture bottom)
    {
        return CreateBox_SideTextures?.Invoke(sx, sy, sz, back, front, left, right, top, bottom);
    }
    public static Func<float, float, float, MTexture, MTexture, MTexture, MTexture, MTexture, MTexture, object> CreateBox_SideTextures;

    /// <summary> Creates a box mesh of the given size, using a separate texture for each side, offset and tinted. </summary>
    public static object createBox_Offset_Color_SideTextures(Vector3 offset, float sx, float sy, float sz, MTexture back, MTexture front, MTexture left, MTexture right, MTexture top, MTexture bottom, Color tint)
    {
        return CreateBox_Offset_Color_SideTextures?.Invoke(offset, sx, sy, sz, back, front, left, right, top, bottom, tint);
    }
    public static Func<Vector3, float, float, float, MTexture, MTexture, MTexture, MTexture, MTexture, MTexture, Color, object> CreateBox_Offset_Color_SideTextures;

    /// <summary> Creates a gear mesh with the given parameters. </summary>
    public static object createGear(float teeth, float depth, float slope, float innerRadius, float thickness, float scale, Color color)
    {
        return CreateGear?.Invoke(teeth, depth, slope, innerRadius, thickness, scale, color);
    }
    public static Func<float, float, float, float, float, float, Color, object> CreateGear;

    /// <summary> Creates a mesh from the given tile voxel data. </summary>
    public static object createTileVoxel(char[,,] voxel)
    {
        return CreateTileVoxel?.Invoke(voxel);
    }
    public static Func<char[,,], object> CreateTileVoxel;

    /// <summary> Creates an arbitrary mesh from the given vertices and indices. </summary>
    public static object createArbitraryMesh(Vector3[] vertices, int[] indices, Color color, float rainbow = 0f, float scale = 1f)
    {
        return CreateArbitraryMesh?.Invoke(vertices, indices, color, rainbow, scale);
    }
    public delegate object CreateArbitraryMeshDelegate(Vector3[] vertices, int[] indices, Color color, float rainbow = 0f, float scale = 1f);
    public static CreateArbitraryMeshDelegate CreateArbitraryMesh;

    /// <summary> Creates an icosahedron mesh. </summary>
    public static object createIcosahedron(Color color, float rainbow = 0f, float scale = 1f)
    {
        return CreateIcosahedron?.Invoke(color, rainbow, scale);
    }
    public delegate object CreateIcosahedronDelegate(Color color, float rainbow = 0f, float scale = 1f);
    public static CreateIcosahedronDelegate CreateIcosahedron;

    /// <summary> Creates an icosphere mesh. </summary>
    public static object createIcosphere1(Color color, float rainbow = 0f, float scale = 1f)
    {
        return CreateIcosphere1?.Invoke(color, rainbow, scale);
    }
    public delegate object CreateIcosphere1Delegate(Color color, float rainbow = 0f, float scale = 1f);
    public static CreateIcosphere1Delegate CreateIcosphere1;

    /// <summary> Creates a rock mesh. </summary>
    public static object createRock(Color color, float rainbow = 0f, float scale = 1f)
    {
        return CreateRock?.Invoke(color, rainbow, scale);
    }
    public delegate object CreateRockDelegate(Color color, float rainbow = 0f, float scale = 1f);
    public static CreateRockDelegate CreateRock;

    /// <summary> Creates a half-ring mesh. </summary>
    public static object createHalfRing(float height, float thickness, Color color)
    {
        return CreateHalfRing?.Invoke(height, thickness, color);
    }
    public static Func<float, float, Color, object> CreateHalfRing;

    /// <summary> Creates a mesh loaded from the given Wavefront .obj model. </summary>
    public static object createFromObj(string modelPath)
    {
        return CreateFromObj?.Invoke(modelPath);
    }
    public static Func<string, object> CreateFromObj;

    /// <summary> Loads a texture from the given path. </summary>
    public static Texture2D createTexture(string texturePath)
    {
        return CreateTexture?.Invoke(texturePath);
    }
    public static Func<string, Texture2D> CreateTexture;

    #endregion
}

#endregion
