int 피해계산(int 공격력, int 방어력) {
    return global::System.Math.Max(공격력 - 방어력, 0);
}

int 체력 = 100;
int 피해 = 피해계산(35, 10);
for (int 차례 = 0; 차례 < (4); 차례++) {
    체력 -= 피해;
    global::System.Console.WriteLine($"{차례 + 1}턴: 남은 체력 {체력}");
    if ((체력) == (0)) {
        global::System.Console.WriteLine("전투 종료");
        break;
    }
}
