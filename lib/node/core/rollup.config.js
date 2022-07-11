import typescript from 'rollup-plugin-typescript2';

export default [
    {
        input: './src/index.ts',
        output: {
            format: 'esm',
            dir: './build/', // 出力先ディレクトリトップ
            entryFileNames: 'index.esm.js',
        },
        plugins: [
            typescript({
                tsconfigOverride: {
                    declaration: true,
                    compilerOptions: {
                        module: "es2015",
                    }
                }
            })
        ]
    }
];