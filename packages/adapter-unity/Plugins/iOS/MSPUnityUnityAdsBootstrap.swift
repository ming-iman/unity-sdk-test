import Foundation
import MSPCore
import UnityAdapter

@_cdecl("msp_unity_register_adapter_unity")
public func msp_unity_register_adapter_unity() {
  MSPUnityEntry.registerManager(UnityManager())
}

@objc(MSPUnityUnityAdsBootstrap)
@objcMembers
public final class MSPUnityUnityAdsBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_unity()
  }
}
