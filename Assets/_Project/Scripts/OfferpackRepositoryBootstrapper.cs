using UnityEngine;

public sealed class OfferpackRepositoryBootstrapper : RepositoryBootstrapper<OfferpackRepository, OfferpackBundleData> { }

public sealed class OfferpackRepository : AssetBundleRepository<OfferpackBundleData>
{
    public static OfferpackRepository Instance { get; private set; }

    public OfferpackRepository(string bundleAddressFormat, bool logEnable, Color logColor) 
        : base(bundleAddressFormat, logEnable, logColor)
    {
        Instance = this;
    }
}