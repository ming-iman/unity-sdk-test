import Foundation
import MSPCore

@objc(MSPUnityEntry)
@objcMembers
public final class MSPUnityEntry: NSObject {
    @objc
    public static func sdkVersion() -> String {
        MSP.shared.version
    }
}
