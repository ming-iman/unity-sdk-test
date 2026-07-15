import Foundation
import MSPCore
import MSPMolocoAdapter

@_cdecl("msp_unity_register_adapter_moloco")
public func msp_unity_register_adapter_moloco() {
  MSPUnityEntry.registerManager(MolocoManager())
  // Same as native iOS demo: wire bid-token provider when Moloco adapter is linked.
  MSP.shared.bidLoaderProvider.molocoBidTokenProvider = MolocoBidTokenProviderHelper()
}

@objc(MSPUnityMolocoBootstrap)
@objcMembers
public final class MSPUnityMolocoBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_moloco()
  }
}
