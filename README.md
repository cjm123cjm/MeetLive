# MeetLive
在线会议

## 读写分离

主库增删改，从表查询==》分库

## 分表

用户发送消息数据太多了，就需要分表

根据什么来分？时间.... 分出来的数据比较均匀的

根据会议来拆分

哈希取模 2的n次方

```java
//对10000数据进行分割，分割到32个表 

for (Integer i = 0; i < 10000; i++) {
    // 1. 生成12位随机字符串
    String random = StringTools.getRandomString(12);
    
    // 2. 计算该字符串的哈希码（绝对值）
    Integer hashCode = Math.abs(random.hashCode());
    
    // 3. 通过取模决定分配到哪个表（1-32号表）
    Integer tableNum = hashCode % 32 + 1;  // +1是为了让表号从1开始
    
    // 4. 统计这个表当前已有多少数据
    Integer count = tempMap.get(tableNum);  // 这里获取该表已有的数据量
    
    // 5. 更新这个表的计数
    if (count == null) {
        tempMap.put(tableNum, 1);  // 如果是第一次出现，设置为1
    } else {
        tempMap.put(tableNum, count + 1);  // 否则在原有基础上+1
    }
}
```

String random = StringTools.getRandomString(12);=》MeetingId

根据MeetingId就可以查询出在哪个表里，就不存在链表查询

前期就要确定要分多少张表，后期不能改



快速找到对应的表、数据可以均匀，

根据业务场景：时间、id哈希分表...

