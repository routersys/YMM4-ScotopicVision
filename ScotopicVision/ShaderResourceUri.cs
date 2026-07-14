namespace ScotopicVision
{
    internal static class ShaderResourceUri
    {
        public static Uri Get(string shaderName) => new($"pack://application:,,,/ScotopicVision;component/Shaders/{shaderName}.cso", UriKind.Absolute);
    }
}
