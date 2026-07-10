import Foundation
import MSPCore
import MSPMolocoAdapter

@_cdecl("msp_unity_register_adapter_moloco")
public func msp_unity_register_adapter_moloco() {
  MSPUnityEntry.registerManager(MolocoManager())
}

@objc(MSPUnityMolocoBootstrap)
@objcMembers
public final class MSPUnityMolocoBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_moloco()
  }
}
