import Foundation
import MSPCore
import MSPFacebookAdapter

@_cdecl("msp_unity_register_adapter_facebook")
public func msp_unity_register_adapter_facebook() {
  MSPUnityEntry.registerManager(FacebookManager())
}

@objc(MSPUnityFacebookBootstrap)
@objcMembers
public final class MSPUnityFacebookBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_facebook()
  }
}
