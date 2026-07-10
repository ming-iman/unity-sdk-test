import Foundation
import MSPCore
import MSPGoogleAdapter

@_cdecl("msp_unity_register_adapter_google")
public func msp_unity_register_adapter_google() {
  MSPUnityEntry.registerManager(GoogleManager())
}

@objc(MSPUnityGoogleBootstrap)
@objcMembers
public final class MSPUnityGoogleBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_google()
  }
}
