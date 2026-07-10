import Foundation
import MSPCore
import InmobiAdapter

@_cdecl("msp_unity_register_adapter_inmobi")
public func msp_unity_register_adapter_inmobi() {
  MSPUnityEntry.registerManager(InmobiManager())
}

@objc(MSPUnityInmobiBootstrap)
@objcMembers
public final class MSPUnityInmobiBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_inmobi()
  }
}
