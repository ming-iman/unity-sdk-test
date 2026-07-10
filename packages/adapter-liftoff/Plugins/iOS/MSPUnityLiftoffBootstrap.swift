import Foundation
import MSPCore
import MSPLiftoffAdapter

@_cdecl("msp_unity_register_adapter_liftoff")
public func msp_unity_register_adapter_liftoff() {
  MSPUnityEntry.registerManager(LiftoffManager())
}

@objc(MSPUnityLiftoffBootstrap)
@objcMembers
public final class MSPUnityLiftoffBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_liftoff()
  }
}
