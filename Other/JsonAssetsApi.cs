namespace MoreRings.Other
{
    public interface JsonAssetsApi
    {
        void LoadAssets(string path);

        int GetObjectId(string name);
    }
}
