# WO 409 series export

`WO 409` series requires additional processing. First, the reference is adjusted to remove the `/16/` and `/Service/1` sections. In addition, medical records references, those ending with `/Medical/1` or `/Medical/2`, are renamed.

| Original | Output |
| --- | --- |
| WO 16/409/27/101/668/Medical/1 | WO 409/27/101/1071 |
| WO 16/409/27/102/20/Medical/1 | WO 409/27/102/1059* |
| WO 16/409/27/102/20/Medical/2 | WO 409/27/102/1059* |
| WO 16/409/27/14/345/Medical/1 | WO 409/27/14/537 |
| WO 16/409/27/30/300/Medical/1 | WO 409/27/30/1058 |
| WO 16/409/27/4/46/Medical/1 | WO 409/27/4/678* |
| WO 16/409/27/4/46/Medical/2 | WO 409/27/4/678* |
| WO 16/409/27/51/301/Medical/1 | WO 409/27/51/738* |
| WO 16/409/27/51/301/Medical/2 | WO 409/27/51/738* |
| WO 16/409/27/70/26/Medical/1 | WO 409/27/70/1074* |
| WO 16/409/27/70/26/Medical/2 | WO 409/27/70/1074* |
| WO 16/409/27/93/12/Medical/1 | WO 409/27/93/662* |
| WO 16/409/27/93/12/Medical/2 | WO 409/27/93/662* |
| WO 16/409/27/93/169/Medical/1 | WO 409/27/93/663* |
| WO 16/409/27/93/169/Medical/2 | WO 409/27/93/663* |
| WO 16/409/27/93/278/Medical/1 | WO 409/27/93/664* |
| WO 16/409/27/93/278/Medical/2 | WO 409/27/93/664* |
| WO 16/409/27/93/319/Medical/1 | WO 409/27/93/665* |
| WO 16/409/27/93/319/Medical/2 | WO 409/27/93/665* |

*Medical records that have multiple representations (`xxx/Medical/1` and `xxx/Medical/2`) are merged into a single output:

- `DigitalFileCount` values are combined
- `DigitalFiles` items are merged into a single array
- `AuditTrail` items are merged into a single array
