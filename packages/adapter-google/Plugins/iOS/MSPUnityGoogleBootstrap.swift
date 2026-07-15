import Foundation
import MSPCore
import MSPGoogleAdapter

@_cdecl("msp_unity_register_adapter_google")
public func msp_unity_register_adapter_google() {
  MSPUnityEntry.registerManager(GoogleManager())
  // Same as native iOS demo: wire query-info fetcher when Google adapter is linked.
  MSP.shared.bidLoaderProvider.googleQueryInfoFetcher = GoogleQueryInfoFetcherHelper()
}

@objc(MSPUnityGoogleBootstrap)
@objcMembers
public final class MSPUnityGoogleBootstrap: NSObject {
  @objc public static func activate() {
    msp_unity_register_adapter_google()
  }
}
