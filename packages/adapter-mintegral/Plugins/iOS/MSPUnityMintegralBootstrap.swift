import Foundation
import MSPCore
import MintegralAdapter

@_cdecl("msp_unity_register_adapter_mintegral")
public func msp_unity_register_adapter_mintegral() {
  MSPUnityEntry.registerManager(MintegralManager())
}

@objc(MSPUnityMintegralBootstrap)
@objcMembers
public final class MSPUnityMintegralBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_mintegral()
  }
}
