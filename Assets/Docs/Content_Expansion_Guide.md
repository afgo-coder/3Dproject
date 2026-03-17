# 상품/손님 타입 확장 가이드

## 추천 상품 8종

`Assets/Product` 아래에 `MiniMart/Product Data`로 만들어 주세요.

1. Water
   - category: `Drink`
   - salePrice: `1000`
   - costPrice: `600`
   - popularity: `1.0`
   - maxShelfCapacity: `8`
2. Cola
   - category: `Drink`
   - salePrice: `1800`
   - costPrice: `1100`
   - popularity: `1.2`
   - maxShelfCapacity: `8`
3. Coffee
   - category: `Drink`
   - salePrice: `2200`
   - costPrice: `1400`
   - popularity: `1.1`
   - maxShelfCapacity: `6`
4. Chips
   - category: `Snack`
   - salePrice: `1500`
   - costPrice: `900`
   - popularity: `1.0`
   - maxShelfCapacity: `10`
5. Chocolate
   - category: `Snack`
   - salePrice: `1300`
   - costPrice: `800`
   - popularity: `0.9`
   - maxShelfCapacity: `10`
6. Cup Noodles
   - category: `InstantFood`
   - salePrice: `2000`
   - costPrice: `1200`
   - popularity: `1.2`
   - maxShelfCapacity: `6`
7. Triangle Kimbap
   - category: `Meal`
   - salePrice: `1700`
   - costPrice: `1000`
   - popularity: `1.3`
   - maxShelfCapacity: `8`
8. Tissue
   - category: `DailyGoods`
   - salePrice: `2500`
   - costPrice: `1500`
   - popularity: `0.7`
   - maxShelfCapacity: `6`

## 추천 손님 타입 4종

`Assets/Customerdata` 아래에 `MiniMart/Customer Type`으로 만들어 주세요.

1. 학생
   - walkSpeed: `2.2`
   - browseDuration: `1.5`
   - preferredProducts:
     - Cola `3`
     - Chips `4`
     - Chocolate `3`
2. 직장인
   - walkSpeed: `2.8`
   - browseDuration: `1.0`
   - preferredProducts:
     - Coffee `5`
     - Triangle Kimbap `4`
     - Water `2`
3. 야식 손님
   - walkSpeed: `2.0`
   - browseDuration: `2.2`
   - preferredProducts:
     - Cup Noodles `5`
     - Cola `2`
     - Water `1`
4. 생활용품 손님
   - walkSpeed: `1.8`
   - browseDuration: `2.5`
   - preferredProducts:
     - Tissue `5`
     - Water `1`

## 씬 연결 순서

1. 상품 에셋을 만든다.
2. `Shelf`를 3~5개로 늘린다.
3. 각 `Shelf`의 `Assigned Product`를 다른 상품으로 지정한다.
4. 손님 타입 에셋을 만든다.
5. `Bootstrap`의 `CustomerManager > Customer Types`에 손님 타입들을 넣는다.
6. `OrderTerminal`을 상품별로 복수 배치하거나, 테스트용으로 상품을 바꿔가며 발주한다.

## 현재 코드 동작

- 손님은 자신의 선호 상품 목록을 우선 확인합니다.
- 선호 상품이 진열된 선반에 재고가 있으면 그 선반을 목표로 잡습니다.
- 선호 상품이 없으면 현재 재고가 있는 아무 선반으로 이동합니다.
