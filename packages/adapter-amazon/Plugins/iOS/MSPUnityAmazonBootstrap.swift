import Foundation
import MSPCore
import MSPAmazonAdapter

@_cdecl("msp_unity_register_adapter_amazon")
public func msp_unity_register_adapter_amazon() {
  MSPUnityEntry.registerManager(AmazonManager())
}

@objc(MSPUnityAmazonBootstrap)
@objcMembers
public final class MSPUnityAmazonBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_amazon()
  }
}
