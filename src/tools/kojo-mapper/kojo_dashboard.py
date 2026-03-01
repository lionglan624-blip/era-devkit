#!/usr/bin/env python3
"""
Kojo Dashboard Generator
Generates an HTML dashboard with charts comparing all characters' kojo data.
"""

import json
import os
from pathlib import Path

# Import kojo_mapper for analysis
import sys
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from kojo_mapper import analyze_character_kojo


def analyze_all_characters(kojo_root: Path) -> list[dict]:
    """Analyze all character directories and return summary data."""
    data = []
    for char_dir in sorted(kojo_root.iterdir()):
        if char_dir.is_dir():
            analysis = analyze_character_kojo(char_dir)
            # Convert to JSON-compatible format
            summary = {
                'character': analysis['character'],
                'summary': {
                    'total_functions': analysis['summary']['total_functions'],
                    'scene_types': dict(analysis['summary']['scene_types']),
                    'favorability_coverage': {k: list(v) for k, v in analysis['summary']['favorability_coverage'].items()},
                    'has_soft_hard': analysis['summary']['has_soft_hard'],
                    'special_conditions': dict(analysis['summary']['special_conditions']),
                    # AC Metrics (Feature 051)
                    'branch_types': dict(analysis['summary']['branch_types']),
                    'branch_depth_counts': dict(analysis['summary']['branch_depth_counts']),
                    'content_line_sum': analysis['summary']['content_line_sum'],
                    'has_variation_count': analysis['summary']['has_variation_count'],
                    'has_else_count': analysis['summary']['has_else_count'],
                    'has_printdata_count': analysis['summary']['has_printdata_count'],
                    # Feature 055: New metrics
                    'printform_line_sum': analysis['summary']['printform_line_sum'],
                    'dialogue_text_sum': analysis['summary']['dialogue_text_sum'],
                    'kojo_block_sum': analysis['summary']['kojo_block_sum'],
                    'talent_lover_count': analysis['summary']['talent_lover_count'],
                    'talent_renbo_count': analysis['summary']['talent_renbo_count'],
                    'talent_shibo_count': analysis['summary']['talent_shibo_count'],
                    'rand_variations_sum': analysis['summary']['rand_variations_sum'],
                }
            }
            data.append(summary)
    return data


def extract_character_name(full_name: str) -> str:
    """Extract clean character name from folder name like '8_チルノ'."""
    if '_' in full_name:
        return full_name.split('_', 1)[1]
    return full_name


def generate_dashboard(data: list[dict], output_path: str):
    """Generate HTML dashboard with Chart.js visualizations."""

    # Prepare data for charts
    characters = []
    total_functions = []
    scene_types_data = {}
    special_conditions_data = {}

    # All possible scene types and conditions
    all_scene_types = set()
    all_conditions = set()

    for item in data:
        char_name = extract_character_name(item['character'])
        characters.append(char_name)
        total_functions.append(item['summary']['total_functions'])

        for scene_type in item['summary']['scene_types'].keys():
            # Replace empty string with "未分類"
            if scene_type == "":
                all_scene_types.add("未分類")
            else:
                all_scene_types.add(scene_type)

        for condition in item['summary']['special_conditions'].keys():
            all_conditions.add(condition)

    all_scene_types = sorted([st for st in all_scene_types if st])  # Filter empty
    all_conditions = sorted(all_conditions)

    # Build scene type matrix
    for scene_type in all_scene_types:
        scene_types_data[scene_type] = []
        for item in data:
            # Handle "未分類" mapping from empty string
            if scene_type == "未分類":
                count = item['summary']['scene_types'].get("", 0)
            else:
                count = item['summary']['scene_types'].get(scene_type, 0)
            scene_types_data[scene_type].append(count)

    # Build special conditions matrix
    for condition in all_conditions:
        special_conditions_data[condition] = []
        for item in data:
            count = item['summary']['special_conditions'].get(condition, 0)
            special_conditions_data[condition].append(count)

    # === AC Metrics Data (Feature 051 + 055) ===
    ac_metrics = {
        'branch_4': [],      # TALENT_4 count
        'branch_3': [],      # TALENT_3 + ABL_3+ count
        'branch_1_2': [],    # TALENT_1, ABL_1, ABL_2 count
        'branch_none': [],   # NONE count
        'avg_lines': [],     # Average PRINTFORM lines (Feature 055)
        'variation': [],     # Has variation count
        'has_else': [],      # Has ELSE count
        'ac_score': [],      # Estimated AC compliance score
        # Feature 055: TALENT level counts
        'talent_lover': [],
        'talent_renbo': [],
        'talent_shibo': [],
        'rand_sum': [],
    }

    for item in data:
        total = item['summary']['total_functions']
        branch_types = item['summary'].get('branch_types', {})

        branch_4 = branch_types.get('TALENT_4', 0)
        branch_3_talent = branch_types.get('TALENT_3', 0)
        branch_3_abl = sum(v for k, v in branch_types.items()
                          if k.startswith('ABL_') and k != 'ABL_1' and k != 'ABL_2')
        branch_1_2 = sum(branch_types.get(k, 0) for k in ['TALENT_1', 'ABL_1', 'ABL_2'])
        branch_none = branch_types.get('NONE', 0)

        ac_metrics['branch_4'].append(branch_4)
        ac_metrics['branch_3'].append(branch_3_talent + branch_3_abl)
        ac_metrics['branch_1_2'].append(branch_1_2)
        ac_metrics['branch_none'].append(branch_none)

        # Updated: Use dialogue text lines / kojo block count
        dialogue_sum = item['summary'].get('dialogue_text_sum', 0)
        block_sum = item['summary'].get('kojo_block_sum', 0)
        avg_lines = dialogue_sum / block_sum if block_sum > 0 else 0
        ac_metrics['avg_lines'].append(round(avg_lines, 1))

        ac_metrics['variation'].append(item['summary'].get('has_variation_count', 0))
        ac_metrics['has_else'].append(item['summary'].get('has_else_count', 0))

        # Feature 055: TALENT level counts
        ac_metrics['talent_lover'].append(item['summary'].get('talent_lover_count', 0))
        ac_metrics['talent_renbo'].append(item['summary'].get('talent_renbo_count', 0))
        ac_metrics['talent_shibo'].append(item['summary'].get('talent_shibo_count', 0))
        ac_metrics['rand_sum'].append(item['summary'].get('rand_variations_sum', 0))

        # Calculate AC score
        if total > 0:
            ac_score = (branch_4 * 100 + (branch_3_talent + branch_3_abl) * 75 + branch_1_2 * 25) / total
        else:
            ac_score = 0
        ac_metrics['ac_score'].append(round(ac_score, 1))

    # Color palette
    colors = [
        'rgba(255, 99, 132, 0.8)',
        'rgba(54, 162, 235, 0.8)',
        'rgba(255, 206, 86, 0.8)',
        'rgba(75, 192, 192, 0.8)',
        'rgba(153, 102, 255, 0.8)',
        'rgba(255, 159, 64, 0.8)',
        'rgba(199, 199, 199, 0.8)',
        'rgba(83, 102, 255, 0.8)',
        'rgba(255, 99, 255, 0.8)',
        'rgba(99, 255, 132, 0.8)',
    ]

    # Generate HTML
    html = f'''<!DOCTYPE html>
<html lang="ja">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>口上マップ ダッシュボード</title>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        * {{
            box-sizing: border-box;
        }}
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            margin: 0;
            padding: 20px;
            background: linear-gradient(135deg, #1a1a2e 0%, #16213e 100%);
            color: #eee;
            min-height: 100vh;
        }}
        h1 {{
            text-align: center;
            color: #fff;
            margin-bottom: 30px;
            text-shadow: 2px 2px 4px rgba(0,0,0,0.3);
        }}
        h2 {{
            color: #fff;
            border-bottom: 2px solid #e94560;
            padding-bottom: 10px;
            margin-top: 40px;
        }}
        .dashboard {{
            max-width: 1400px;
            margin: 0 auto;
        }}
        .chart-container {{
            background: rgba(255,255,255,0.05);
            border-radius: 15px;
            padding: 20px;
            margin-bottom: 30px;
            box-shadow: 0 8px 32px rgba(0,0,0,0.3);
        }}
        .chart-row {{
            display: grid;
            grid-template-columns: 1fr 1fr;
            gap: 20px;
        }}
        @media (max-width: 900px) {{
            .chart-row {{
                grid-template-columns: 1fr;
            }}
        }}
        .summary-table {{
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }}
        .summary-table th, .summary-table td {{
            padding: 12px;
            text-align: center;
            border: 1px solid rgba(255,255,255,0.1);
        }}
        .summary-table th {{
            background: rgba(233, 69, 96, 0.3);
            color: #fff;
        }}
        .summary-table tr:nth-child(even) {{
            background: rgba(255,255,255,0.05);
        }}
        .summary-table tr:hover {{
            background: rgba(255,255,255,0.1);
        }}
        canvas {{
            max-height: 400px;
        }}
    </style>
</head>
<body>
    <div class="dashboard">
        <h1>🎭 口上マップ ダッシュボード</h1>

        <h2>📊 総関数数比較</h2>
        <div class="chart-container">
            <canvas id="totalChart"></canvas>
        </div>

        <h2>✅ AC準拠スコア</h2>
        <div class="chart-container">
            <canvas id="acScoreChart"></canvas>
        </div>

        <h2>🔀 関係性分岐タイプ</h2>
        <div class="chart-container">
            <canvas id="branchStackedChart"></canvas>
        </div>

        <div class="chart-row">
            <div class="chart-container">
                <canvas id="variationChart"></canvas>
            </div>
            <div class="chart-container">
                <canvas id="avgLinesChart"></canvas>
            </div>
        </div>

        <h2>🎬 シーンタイプ別分布</h2>
        <div class="chart-container">
            <canvas id="sceneStackedChart"></canvas>
        </div>

        <div class="chart-row">
            <div class="chart-container">
                <canvas id="sceneRadarChart"></canvas>
            </div>
            <div class="chart-container">
                <canvas id="scenePieChart"></canvas>
            </div>
        </div>

        <h2>🔀 特殊条件分岐</h2>
        <div class="chart-container">
            <canvas id="conditionsChart"></canvas>
        </div>

        <div class="chart-row">
            <div class="chart-container">
                <canvas id="conditionsRadarChart"></canvas>
            </div>
            <div class="chart-container">
                <canvas id="conditionsPieChart"></canvas>
            </div>
        </div>

        <h2>📋 サマリーテーブル</h2>
        <div class="chart-container">
            <table class="summary-table">
                <thead>
                    <tr>
                        <th>キャラクター</th>
                        <th>総関数数</th>
                        {' '.join(f'<th>{st}</th>' for st in all_scene_types)}
                    </tr>
                </thead>
                <tbody>
'''

    for i, item in enumerate(data):
        char_name = extract_character_name(item['character'])
        html += f'''                    <tr>
                        <td><strong>{char_name}</strong></td>
                        <td>{item['summary']['total_functions']}</td>
'''
        for st in all_scene_types:
            if st == "未分類":
                count = item['summary']['scene_types'].get("", 0)
            else:
                count = item['summary']['scene_types'].get(st, 0)
            html += f'                        <td>{count}</td>\n'
        html += '                    </tr>\n'

    html += f'''                </tbody>
            </table>
        </div>

        <h2>🎯 特殊条件テーブル</h2>
        <div class="chart-container">
            <table class="summary-table">
                <thead>
                    <tr>
                        <th>キャラクター</th>
                        {' '.join(f'<th>{c}</th>' for c in all_conditions)}
                    </tr>
                </thead>
                <tbody>
'''

    for i, item in enumerate(data):
        char_name = extract_character_name(item['character'])
        html += f'''                    <tr>
                        <td><strong>{char_name}</strong></td>
'''
        for c in all_conditions:
            count = item['summary']['special_conditions'].get(c, 0)
            html += f'                        <td>{count if count > 0 else "-"}</td>\n'
        html += '                    </tr>\n'

    # AC Metrics Table (Feature 051)
    html += f'''                </tbody>
            </table>
        </div>

        <h2>✅ AC準拠テーブル</h2>
        <div class="chart-container">
            <table class="summary-table">
                <thead>
                    <tr>
                        <th>キャラクター</th>
                        <th>AC Score</th>
                        <th>4段階</th>
                        <th>3段階</th>
                        <th>1-2段階</th>
                        <th>分岐なし</th>
                        <th>口上行/分岐</th>
                        <th>恋人</th>
                        <th>恋慕</th>
                        <th>思慕</th>
                        <th>RAND</th>
                        <th>Var</th>
                        <th>ELSE</th>
                    </tr>
                </thead>
                <tbody>
'''
    for i, item in enumerate(data):
        char_name = extract_character_name(item['character'])
        html += f'''                    <tr>
                        <td><strong>{char_name}</strong></td>
                        <td style="color: {'#4ade80' if ac_metrics['ac_score'][i] >= 50 else '#fbbf24' if ac_metrics['ac_score'][i] >= 25 else '#f87171'}">{ac_metrics['ac_score'][i]}%</td>
                        <td>{ac_metrics['branch_4'][i]}</td>
                        <td>{ac_metrics['branch_3'][i]}</td>
                        <td>{ac_metrics['branch_1_2'][i]}</td>
                        <td>{ac_metrics['branch_none'][i]}</td>
                        <td>{ac_metrics['avg_lines'][i]}</td>
                        <td>{ac_metrics['talent_lover'][i]}</td>
                        <td>{ac_metrics['talent_renbo'][i]}</td>
                        <td>{ac_metrics['talent_shibo'][i]}</td>
                        <td>{ac_metrics['rand_sum'][i]}</td>
                        <td>{ac_metrics['variation'][i]}</td>
                        <td>{ac_metrics['has_else'][i]}</td>
                    </tr>
'''

    # Generate scene type datasets for stacked bar
    scene_datasets = []
    for i, st in enumerate(all_scene_types):
        scene_datasets.append({
            'label': st,
            'data': scene_types_data[st],
            'backgroundColor': colors[i % len(colors)]
        })

    # Generate condition datasets
    condition_datasets = []
    for i, c in enumerate(all_conditions):
        condition_datasets.append({
            'label': c,
            'data': special_conditions_data[c],
            'backgroundColor': colors[i % len(colors)]
        })

    # Aggregate totals for pie charts
    scene_totals = {st: sum(scene_types_data[st]) for st in all_scene_types}
    condition_totals = {c: sum(special_conditions_data[c]) for c in all_conditions}

    html += f'''                </tbody>
            </table>
        </div>
    </div>

    <script>
        const characters = {json.dumps(characters, ensure_ascii=False)};
        const totalFunctions = {json.dumps(total_functions)};
        const sceneTypes = {json.dumps(all_scene_types, ensure_ascii=False)};
        const conditions = {json.dumps(all_conditions, ensure_ascii=False)};
        const colors = {json.dumps(colors)};

        // AC Metrics Data (Feature 051)
        const acScores = {json.dumps(ac_metrics['ac_score'])};
        const branch4 = {json.dumps(ac_metrics['branch_4'])};
        const branch3 = {json.dumps(ac_metrics['branch_3'])};
        const branch1_2 = {json.dumps(ac_metrics['branch_1_2'])};
        const branchNone = {json.dumps(ac_metrics['branch_none'])};
        const avgLines = {json.dumps(ac_metrics['avg_lines'])};
        const variationCount = {json.dumps(ac_metrics['variation'])};
        const elseCount = {json.dumps(ac_metrics['has_else'])};

        // Total Functions Bar Chart
        new Chart(document.getElementById('totalChart'), {{
            type: 'bar',
            data: {{
                labels: characters,
                datasets: [{{
                    label: '総関数数',
                    data: totalFunctions,
                    backgroundColor: colors.slice(0, characters.length),
                    borderColor: colors.slice(0, characters.length).map(c => c.replace('0.8', '1')),
                    borderWidth: 2
                }}]
            }},
            options: {{
                responsive: true,
                plugins: {{
                    legend: {{ display: false }},
                    title: {{ display: true, text: 'キャラクター別 総関数数', color: '#fff', font: {{ size: 16 }} }}
                }},
                scales: {{
                    y: {{ beginAtZero: true, ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }},
                    x: {{ ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }}
                }}
            }}
        }});

        // AC Score Bar Chart (Feature 051)
        new Chart(document.getElementById('acScoreChart'), {{
            type: 'bar',
            data: {{
                labels: characters,
                datasets: [{{
                    label: 'AC準拠スコア (%)',
                    data: acScores,
                    backgroundColor: acScores.map(s => s >= 50 ? 'rgba(74, 222, 128, 0.8)' : s >= 25 ? 'rgba(251, 191, 36, 0.8)' : 'rgba(248, 113, 113, 0.8)'),
                    borderColor: acScores.map(s => s >= 50 ? 'rgba(74, 222, 128, 1)' : s >= 25 ? 'rgba(251, 191, 36, 1)' : 'rgba(248, 113, 113, 1)'),
                    borderWidth: 2
                }}]
            }},
            options: {{
                responsive: true,
                plugins: {{
                    legend: {{ display: false }},
                    title: {{ display: true, text: 'AC準拠スコア（推定）', color: '#fff', font: {{ size: 16 }} }}
                }},
                scales: {{
                    y: {{ beginAtZero: true, max: 100, ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }},
                    x: {{ ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }}
                }}
            }}
        }});

        // Branch Types Stacked Bar Chart (Feature 051)
        new Chart(document.getElementById('branchStackedChart'), {{
            type: 'bar',
            data: {{
                labels: characters,
                datasets: [
                    {{ label: '4段階分岐', data: branch4, backgroundColor: 'rgba(74, 222, 128, 0.8)' }},
                    {{ label: '3段階分岐', data: branch3, backgroundColor: 'rgba(54, 162, 235, 0.8)' }},
                    {{ label: '1-2段階分岐', data: branch1_2, backgroundColor: 'rgba(251, 191, 36, 0.8)' }},
                    {{ label: '分岐なし', data: branchNone, backgroundColor: 'rgba(248, 113, 113, 0.8)' }}
                ]
            }},
            options: {{
                responsive: true,
                plugins: {{
                    legend: {{ labels: {{ color: '#ccc' }} }},
                    title: {{ display: true, text: '関係性分岐タイプ（積み上げ）', color: '#fff', font: {{ size: 16 }} }}
                }},
                scales: {{
                    x: {{ stacked: true, ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }},
                    y: {{ stacked: true, ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }}
                }}
            }}
        }});

        // Variation Count Bar Chart (Feature 051)
        new Chart(document.getElementById('variationChart'), {{
            type: 'bar',
            data: {{
                labels: characters,
                datasets: [
                    {{ label: 'バリエーション', data: variationCount, backgroundColor: 'rgba(153, 102, 255, 0.8)' }},
                    {{ label: 'ELSE分岐', data: elseCount, backgroundColor: 'rgba(255, 159, 64, 0.8)' }}
                ]
            }},
            options: {{
                responsive: true,
                plugins: {{
                    legend: {{ labels: {{ color: '#ccc' }} }},
                    title: {{ display: true, text: 'バリエーション・ELSE分岐', color: '#fff', font: {{ size: 16 }} }}
                }},
                scales: {{
                    y: {{ beginAtZero: true, ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }},
                    x: {{ ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }}
                }}
            }}
        }});

        // Average Lines Bar Chart (Feature 051)
        new Chart(document.getElementById('avgLinesChart'), {{
            type: 'bar',
            data: {{
                labels: characters,
                datasets: [{{
                    label: '平均行数',
                    data: avgLines,
                    backgroundColor: avgLines.map(l => l >= 4 && l <= 8 ? 'rgba(74, 222, 128, 0.8)' : l < 4 ? 'rgba(251, 191, 36, 0.8)' : 'rgba(54, 162, 235, 0.8)'),
                    borderWidth: 2
                }}]
            }},
            options: {{
                responsive: true,
                plugins: {{
                    legend: {{ display: false }},
                    title: {{ display: true, text: '平均口上行数/分岐（目標: 4+行）', color: '#fff', font: {{ size: 16 }} }},
                    annotation: {{
                        annotations: {{
                            line1: {{ type: 'line', yMin: 4, yMax: 4, borderColor: 'rgba(74, 222, 128, 0.5)', borderWidth: 2, borderDash: [5, 5] }},
                            line2: {{ type: 'line', yMin: 8, yMax: 8, borderColor: 'rgba(74, 222, 128, 0.5)', borderWidth: 2, borderDash: [5, 5] }}
                        }}
                    }}
                }},
                scales: {{
                    y: {{ beginAtZero: true, ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }},
                    x: {{ ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }}
                }}
            }}
        }});

        // Scene Types Stacked Bar Chart
        new Chart(document.getElementById('sceneStackedChart'), {{
            type: 'bar',
            data: {{
                labels: characters,
                datasets: {json.dumps(scene_datasets, ensure_ascii=False)}
            }},
            options: {{
                responsive: true,
                plugins: {{
                    legend: {{ labels: {{ color: '#ccc' }} }},
                    title: {{ display: true, text: 'シーンタイプ別 関数数（積み上げ）', color: '#fff', font: {{ size: 16 }} }}
                }},
                scales: {{
                    x: {{ stacked: true, ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }},
                    y: {{ stacked: true, ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }}
                }}
            }}
        }});

        // Scene Types Radar Chart
        const sceneData = {json.dumps({st: scene_types_data[st] for st in all_scene_types}, ensure_ascii=False)};
        new Chart(document.getElementById('sceneRadarChart'), {{
            type: 'radar',
            data: {{
                labels: sceneTypes,
                datasets: characters.map((char, i) => ({{
                    label: char,
                    data: sceneTypes.map(st => sceneData[st][i]),
                    borderColor: colors[i % colors.length].replace('0.8', '1'),
                    backgroundColor: colors[i % colors.length].replace('0.8', '0.2'),
                    pointBackgroundColor: colors[i % colors.length]
                }}))
            }},
            options: {{
                responsive: true,
                plugins: {{
                    legend: {{ labels: {{ color: '#ccc' }} }},
                    title: {{ display: true, text: 'シーンタイプ レーダーチャート', color: '#fff', font: {{ size: 16 }} }}
                }},
                scales: {{
                    r: {{
                        angleLines: {{ color: 'rgba(255,255,255,0.2)' }},
                        grid: {{ color: 'rgba(255,255,255,0.2)' }},
                        pointLabels: {{ color: '#ccc' }},
                        ticks: {{ color: '#ccc', backdropColor: 'transparent' }}
                    }}
                }}
            }}
        }});

        // Scene Types Pie Chart (Total)
        new Chart(document.getElementById('scenePieChart'), {{
            type: 'doughnut',
            data: {{
                labels: {json.dumps(list(scene_totals.keys()), ensure_ascii=False)},
                datasets: [{{
                    data: {json.dumps(list(scene_totals.values()))},
                    backgroundColor: colors.slice(0, {len(scene_totals)})
                }}]
            }},
            options: {{
                responsive: true,
                plugins: {{
                    legend: {{ labels: {{ color: '#ccc' }} }},
                    title: {{ display: true, text: 'シーンタイプ全体比率', color: '#fff', font: {{ size: 16 }} }}
                }}
            }}
        }});

        // Conditions Stacked Bar Chart
        new Chart(document.getElementById('conditionsChart'), {{
            type: 'bar',
            data: {{
                labels: characters,
                datasets: {json.dumps(condition_datasets, ensure_ascii=False)}
            }},
            options: {{
                responsive: true,
                plugins: {{
                    legend: {{ labels: {{ color: '#ccc' }} }},
                    title: {{ display: true, text: '特殊条件分岐（積み上げ）', color: '#fff', font: {{ size: 16 }} }}
                }},
                scales: {{
                    x: {{ stacked: true, ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }},
                    y: {{ stacked: true, ticks: {{ color: '#ccc' }}, grid: {{ color: 'rgba(255,255,255,0.1)' }} }}
                }}
            }}
        }});

        // Conditions Radar Chart
        const conditionsData = {json.dumps({c: special_conditions_data[c] for c in all_conditions}, ensure_ascii=False)};
        new Chart(document.getElementById('conditionsRadarChart'), {{
            type: 'radar',
            data: {{
                labels: conditions,
                datasets: characters.map((char, i) => ({{
                    label: char,
                    data: conditions.map(c => conditionsData[c][i]),
                    borderColor: colors[i % colors.length].replace('0.8', '1'),
                    backgroundColor: colors[i % colors.length].replace('0.8', '0.2'),
                    pointBackgroundColor: colors[i % colors.length]
                }}))
            }},
            options: {{
                responsive: true,
                plugins: {{
                    legend: {{ labels: {{ color: '#ccc' }} }},
                    title: {{ display: true, text: '特殊条件 レーダーチャート', color: '#fff', font: {{ size: 16 }} }}
                }},
                scales: {{
                    r: {{
                        angleLines: {{ color: 'rgba(255,255,255,0.2)' }},
                        grid: {{ color: 'rgba(255,255,255,0.2)' }},
                        pointLabels: {{ color: '#ccc' }},
                        ticks: {{ color: '#ccc', backdropColor: 'transparent' }}
                    }}
                }}
            }}
        }});

        // Conditions Pie Chart (Total)
        new Chart(document.getElementById('conditionsPieChart'), {{
            type: 'doughnut',
            data: {{
                labels: {json.dumps(list(condition_totals.keys()), ensure_ascii=False)},
                datasets: [{{
                    data: {json.dumps(list(condition_totals.values()))},
                    backgroundColor: colors.slice(0, {len(condition_totals)})
                }}]
            }},
            options: {{
                responsive: true,
                plugins: {{
                    legend: {{ labels: {{ color: '#ccc' }} }},
                    title: {{ display: true, text: '特殊条件全体比率', color: '#fff', font: {{ size: 16 }} }}
                }}
            }}
        }});
    </script>
</body>
</html>
'''

    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(html)

    print(f"Generated: {output_path}")


def main():
    # Find Game/ERB/口上 directory relative to script location
    script_dir = Path(os.path.dirname(os.path.abspath(__file__)))
    game_root = script_dir.parent.parent.parent / "Game"
    kojo_root = game_root / "ERB" / "口上"

    if not kojo_root.exists():
        print(f"Error: {kojo_root} not found")
        return

    output_path = script_dir / "kojo-dashboard.html"

    print(f"Analyzing characters in {kojo_root}...")
    data = analyze_all_characters(kojo_root)
    if not data:
        print(f"No character directories found in {kojo_root}")
        return

    print(f"Analyzed {len(data)} characters")
    generate_dashboard(data, str(output_path))


if __name__ == "__main__":
    main()
