import Foundation
import MSPCore
import MSPApplovinMaxAdapter

@_cdecl("msp_unity_register_adapter_applovin")
public func msp_unity_register_adapter_applovin() {
  MSPUnityEntry.registerManager(ApplovinMaxManager())
}

@objc(MSPUnityApplovinBootstrap)
@objcMembers
public final class MSPUnityApplovinBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_applovin()
  }
}
