import Foundation
import MSPCore
import MSPNovaAdapter

@_cdecl("msp_unity_register_adapter_nova")
public func msp_unity_register_adapter_nova() {
  MSPUnityEntry.registerManager(NovaManager())
}

@objc(MSPUnityNovaBootstrap)
@objcMembers
public final class MSPUnityNovaBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_nova()
  }
}
