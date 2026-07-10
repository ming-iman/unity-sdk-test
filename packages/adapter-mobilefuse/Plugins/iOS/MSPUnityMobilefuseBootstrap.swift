import Foundation
import MSPCore
import MobilefuseAdapter

@_cdecl("msp_unity_register_adapter_mobilefuse")
public func msp_unity_register_adapter_mobilefuse() {
  MSPUnityEntry.registerManager(MobilefuseManager())
}

@objc(MSPUnityMobilefuseBootstrap)
@objcMembers
public final class MSPUnityMobilefuseBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_mobilefuse()
  }
}
