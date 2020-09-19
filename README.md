# CrackUnityHub



### 效果

![image-20200920025022069](Image/image-20200920025022069.png)



### 操作方法

```
下载 UnityHub2.4.1.0/app.7z，解压得到 app.asar 文件，然后替换掉 "Your UnityHub Folder/resources/app.asar" 文件即可。
```



### 代码修改逻辑


1. 先解包：```asar extract app.asar app```

2. 
    ```javascript
    修改 build\services\licenseService\licenseClient.js 中的 getLicenseInfo 方法为如下代码：
    	getLicenseInfo(callback) {
            licenseInfo.activated = true;
            licenseInfo.flow = licenseCore.licenseKinds.PRO;
            licenseInfo.label = licenseCore.licenseKinds.PRO;
            licenseInfo.offlineDisabled = false;
            licenseInfo.transactionId = licenseCore.getTransactionId();
            licenseInfo.startDate = new Date('1993-01-01T08:00:00.000Z');
            licenseInfo.stopDate = licenseCore.getInfinityDate();
            licenseInfo.displayedStopDate = false;
            licenseInfo.canExpire = false;
            const licenseInfoString = JSON.stringify(licenseInfo);
            if (callback !== undefined) {
                callback(undefined, licenseInfoString);
            }
            return Promise.resolve(licenseInfoString);
        }
    ```

3. ```javascript
   修改 build\services\localAuth\auth.js 中的 getDefaultUserInfo 方法为如下代码：
   	static getDefaultUserInfo() {
           return {
               accessToken: '',
               displayName: 'anonymous',
               organizationForeignKeys: '',
               primaryOrg: '',
               userId: 'anonymous',
               name: 'anonymous',
               valid: false,
               whitelisted: true
           };
       }
   ```

4. 最后打包即可：```asar pack app app.asar```



