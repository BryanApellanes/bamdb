var database = {
    nameSpace: "Bam.TestDaoClasses",
    schemaName: "TestDaoClasses",
    xrefs: [
        ["Left", "Right"]
    ],
    tables: [
        {
            name: "Left",
            cols: [
                { Name: "String", Null: false },
                { Description: "String" }
            ]
        },
        {
            name: "Right",
            cols: [
                { Name: "String", Null: false },
                { Description: "String" }
            ]
        },
        {
            name: "TestTable",
            cols: [
                { Name: "String", Null: false },
                { Description: "String" }
            ]
        },
        {
            name: "TestFkTable",
            fks: [
                { TestTableId: "TestTable" }
            ],
            cols: [
                { Name: "String", Null: false }
            ]
        },
        {
            name: "DaoReferenceObject",
            cols: [
                { BoolProperty: "Boolean" },
                { IntProperty: "Int" },
                { UIntProperty: "UInt" },
                { ULongProperty: "ULong" },
                { LogProperty: "Long" },
                { DecimalProperty: "Decimal" },
                { StringProperty: "String" }
                { ByteArrayProperty: "ByteArray" },
                { DateTimeProperty: "DateTime" }
            ]
        },
        {
            name: "DaoReferenceObjectWithForeignKey",
            fks: [
                { DaoReferenceObjectId: "DaoReferenceObject" }
            ],
            cols: [
                { Name: "String", Null: false },
                { Description: "String" }
            ]
        }
    ]
}