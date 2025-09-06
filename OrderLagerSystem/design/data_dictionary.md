
# Data Dictionary (logisk nivå)

Attributtyper är konceptuella: **string**, **integer**, **datetime**, **boolean**. `PK` = primärnyckel, `FK` = främmande nyckel, `UQ` = unikt.

---

## User
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| UserId | integer | PK | ja | Teknisk nyckel |
| Email | string | UQ | ja | Inloggnings-/kontaktadress |
| DisplayName | string |  | ja | Visningsnamn |
| IsActive | boolean |  | ja | Aktiv/inaktiv |
| CreatedAt | datetime |  | ja | Skapad-tid |

## Role
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| RoleId | integer | PK | ja | Teknisk nyckel |
| Name | string | UQ | ja | Rollenamn |

## UserRole (kopplingstabell)
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| UserId | integer | PK, FK→User | ja | Användare |
| RoleId | integer | PK, FK→Role | ja | Roll |

## Customer
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| CustomerId | integer | PK | ja | Kund |
| Name | string |  | ja | Kundnamn |
| Email | string |  | nej | E-post |
| Phone | string |  | nej | Telefon |

## Supplier
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| SupplierId | integer | PK | ja | Leverantör |
| Name | string |  | ja | Namn |
| Email | string |  | nej | E-post |
| Phone | string |  | nej | Telefon |

## Category
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| CategoryId | integer | PK | ja | Kategori |
| Name | string | UQ | ja | Namn |

## Product
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| ProductId | integer | PK | ja | Artikel |
| SKU | string | UQ | ja | Artikelnummer |
| Name | string |  | ja | Benämning |
| CategoryId | integer | FK→Category | nej | Kategori |
| IsActive | boolean |  | ja | Aktiv/inaktiv |
| CreatedAt | datetime |  | ja | Skapad-tid |

## Location
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| LocationId | integer | PK | ja | Lagerplats/lager |
| Code | string | UQ | ja | Kod, unik |
| Description | string |  | nej | Beskrivning |
| IsActive | boolean |  | ja | Aktiv/inaktiv |

## Inventory (saldo per produkt & plats)
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| ProductId | integer | PK, FK→Product | ja | Produkt |
| LocationId | integer | PK, FK→Location | ja | Plats |
| OnHandQty | integer |  | ja | Aktuellt saldo |

## InventoryTransaction (transaktionslogg)
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| InventoryTransactionId | integer | PK | ja | Teknisk nyckel |
| ProductId | integer | FK→Product | ja | Produkt |
| LocationId | integer | FK→Location | ja | Plats |
| Quantity | integer |  | ja | +in / -ut |
| MovementType | string |  | ja | RECEIPT/SHIPMENT/ADJUSTMENT/... |
| RelatedTable | string |  | nej | Ursprungsentitet |
| RelatedId | integer |  | nej | Nyckel i ursprung |
| PerformedBy | integer | FK→User | nej | Vem utförde |
| OccurredAt | datetime |  | ja | Tidpunkt |

## OrderStatus
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| OrderStatusId | integer | PK | ja | Status |
| Name | string | UQ | ja | Namn |

## Order
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| OrderId | integer | PK | ja | Order |
| OrderNumber | string | UQ | ja | Externt ordernr |
| CustomerId | integer | FK→Customer | ja | Kund |
| CurrentStatusId | integer | FK→OrderStatus | ja | Aktuell status |
| CreatedAt | datetime |  | ja | Skapad-tid |
| CreatedBy | integer | FK→User | nej | Skapad av |

## OrderLine
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| OrderLineId | integer | PK | ja | Rad |
| OrderId | integer | FK→Order | ja | Tillhör order |
| ProductId | integer | FK→Product | ja | Artikel |
| Quantity | integer |  | ja | Antal (>0) |

## OrderStatusHistory
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| OrderStatusHistoryId | integer | PK | ja | Historikrad |
| OrderId | integer | FK→Order | ja | Order |
| FromStatusId | integer | FK→OrderStatus | nej | Från status |
| ToStatusId | integer | FK→OrderStatus | ja | Till status |
| ChangedAt | datetime |  | ja | Tidpunkt |
| ChangedBy | integer | FK→User | nej | Utförare |
| Note | string |  | nej | Kommentar |

## Receipt (inleverans)
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| ReceiptId | integer | PK | ja | Inleverans |
| SupplierId | integer | FK→Supplier | nej | Leverantör |
| ReceivedAt | datetime |  | ja | Mottaget |
| ReceivedBy | integer | FK→User | nej | Mottaget av |
| Note | string |  | nej | Kommentar |

## ReceiptLine
| Fält | Typ | Nyckel | Krävs | Beskrivning |
|---|---|---|---|---|
| ReceiptLineId | integer | PK | ja | Rad |
| ReceiptId | integer | FK→Receipt | ja | Inleverans |
| ProductId | integer | FK→Product | ja | Artikel |
| LocationId | integer | FK→Location | ja | Plats |
| Quantity | integer |  | ja | Antal (>0) |

---

## Relationsmatris (cardinality)
- User (1) —< UserRole >— (1) Role  
- Customer (1) —< Order  
- Order (1) —< OrderLine  
- Product (1) —< OrderLine  
- OrderStatus (1) —< OrderStatusHistory  
- Order (1) —< OrderStatusHistory; Order (1) — (1) OrderStatus (CurrentStatusId)  
- Product (1) —< Inventory >— (1) Location  
- Product (1) —< InventoryTransaction >— (1) Location; User (0..1) —< InventoryTransaction  
- Supplier (0..1) —< Receipt (0..n) —< ReceiptLine >— Product/Location
