using UnityEngine;

public sealed class OfferpackRepositoryBootstrapper : RepositoryBootstrapper<OfferpackRepository, OfferpackBundleData> 
{
    protected override void CreateRepository()
    {
        new OfferpackRepository(bundleAddressFormat, logEnabled, logColor);
    }

    protected override OfferpackRepository GetRepositoryInstance()
    {
        return OfferpackRepository.Instance;
    }
}

public sealed class OfferpackRepository : AssetBundleRepository<OfferpackBundleData>
{
    public static OfferpackRepository Instance { get; private set; }

    public OfferpackRepository(string bundleAddressFormat, bool logEnable, Color logColor) 
        : base(bundleAddressFormat, logEnable, logColor)
    {
        Instance = this;
    }

    public override void Dispose()
    {
        base.Dispose();

        Instance = null;
    }
}