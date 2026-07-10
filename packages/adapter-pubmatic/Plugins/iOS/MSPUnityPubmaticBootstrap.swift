import Foundation
import MSPCore
import PubmaticAdapter

@_cdecl("msp_unity_register_adapter_pubmatic")
public func msp_unity_register_adapter_pubmatic() {
  MSPUnityEntry.registerManager(PubmaticManager())
}

@objc(MSPUnityPubmaticBootstrap)
@objcMembers
public final class MSPUnityPubmaticBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_pubmatic()
  }
}
