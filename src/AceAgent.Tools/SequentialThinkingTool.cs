using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AceAgent.Core.Interfaces;
using AceAgent.Core.Models;

namespace AceAgent.Tools
{
    /// <summary>
    /// 顺序思维推理工具
    /// 用于复杂问题的分析和解决，支持多步骤思考过程
    /// </summary>
    public class SequentialThinkingTool : ITool
    {
        /// <summary>
        /// 工具名称
        /// </summary>
        public string Name => "sequentialthinking";
        
        /// <summary>
        /// 工具描述
        /// </summary>
        public string Description => "顺序思维推理工具，用于复杂问题的分析和解决，支持多步骤思考过程";

        /// <summary>
        /// 执行顺序思维工具
        /// </summary>
        /// <param name="input">工具输入</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <returns>工具执行结果</returns>
        public async Task<ToolResult> ExecuteAsync(ToolInput input, CancellationToken cancellationToken = default)
        {
            try
            {
                var problem = input.GetParameter<string>("problem");
                var maxSteps = input.GetParameter<int?>("max_steps") ?? 10;
                var context = input.GetParameter<string>("context") ?? "";
                var constraints = input.GetParameter<List<string>>("constraints") ?? new List<string>();
                var goals = input.GetParameter<List<string>>("goals") ?? new List<string>();

                if (string.IsNullOrWhiteSpace(problem))
                {
                    return ToolResult.Failure("问题描述不能为空");
                }

                var reasoningResult = await PerformReasoningAsync(problem, maxSteps, context, constraints, goals);
                
                var result = ToolResult.CreateSuccess(
                    "推理完成",
                    reasoningResult
                );
                result.Metadata["total_steps"] = reasoningResult.Steps.Count;
                result.Metadata["confidence"] = reasoningResult.Confidence;
                result.Metadata["reasoning_time"] = reasoningResult.ReasoningTime.TotalMilliseconds;
                return result;
            }
            catch (Exception ex)
            {
                return ToolResult.FromException(ex);
            }
        }

        /// <summary>
        /// 验证输入参数
        /// </summary>
        /// <param name="input">工具输入</param>
        /// <returns>验证结果</returns>
        public Task<bool> ValidateInputAsync(ToolInput input)
        {
            var problem = input.GetParameter<string>("problem");
            var maxSteps = input.GetParameter<int?>("max_steps") ?? 10;

            var isValid = !string.IsNullOrWhiteSpace(problem) && maxSteps > 0 && maxSteps <= 50;
            return Task.FromResult(isValid);
        }

        private async Task<ReasoningResult> PerformReasoningAsync(
            string problem, 
            int maxSteps, 
            string context, 
            List<string> constraints, 
            List<string> goals)
        {
            var startTime = DateTime.UtcNow;
            var steps = new List<ReasoningStep>();
            var currentHypotheses = new List<string>();
            var evidence = new List<string>();
            var alternatives = new List<string>();

            // 步骤1：问题分析
            var analysisStep = await AnalyzeProblemAsync(problem, context, constraints, goals);
            steps.Add(analysisStep);
            currentHypotheses.AddRange(analysisStep.GeneratedHypotheses);

            // 步骤2-N：迭代推理
            for (int i = 1; i < maxSteps && currentHypotheses.Any(); i++)
            {
                var reasoningStep = await PerformReasoningStepAsync(
                    i + 1, 
                    problem, 
                    currentHypotheses, 
                    evidence, 
                    steps.LastOrDefault()?.Conclusion ?? ""
                );
                
                steps.Add(reasoningStep);
                
                // 更新假设和证据
                if (reasoningStep.GeneratedHypotheses.Any())
                {
                    currentHypotheses = reasoningStep.GeneratedHypotheses;
                }
                
                evidence.AddRange(reasoningStep.Evidence);
                alternatives.AddRange(reasoningStep.Alternatives);

                // 检查是否达到结论
                if (reasoningStep.IsConclusive)
                {
                    break;
                }
            }

            // 最终综合
            var finalStep = await SynthesizeResultAsync(problem, steps, evidence, alternatives);
            steps.Add(finalStep);

            var endTime = DateTime.UtcNow;
            var confidence = CalculateConfidence(steps, evidence.Count, alternatives.Count);

            return new ReasoningResult
            {
                Problem = problem,
                Steps = steps,
                FinalConclusion = finalStep.Conclusion,
                Confidence = confidence,
                Evidence = evidence,
                Alternatives = alternatives,
                ReasoningTime = endTime - startTime,
                Metadata = new Dictionary<string, object>
                {
                    ["context"] = context,
                    ["constraints"] = constraints,
                    ["goals"] = goals,
                    ["total_hypotheses_generated"] = steps.Sum(s => s.GeneratedHypotheses.Count),
                    ["evidence_count"] = evidence.Count,
                    ["alternatives_count"] = alternatives.Count
                }
            };
        }

        private async Task<ReasoningStep> AnalyzeProblemAsync(
            string problem, 
            string context, 
            List<string> constraints, 
            List<string> goals)
        {
            await Task.Delay(10); // 模拟思考时间

            var step = new ReasoningStep
            {
                StepNumber = 1,
                Type = ReasoningStepType.Analysis,
                Description = "问题分析和初始假设生成",
                Input = problem,
                Reasoning = $"分析问题：{problem}\n" +
                           $"上下文：{context}\n" +
                           $"约束条件：{string.Join(", ", constraints)}\n" +
                           $"目标：{string.Join(", ", goals)}",
                Conclusion = "已识别问题的关键要素和可能的解决方向",
                Confidence = 0.7,
                GeneratedHypotheses = GenerateInitialHypotheses(problem, context, goals),
                Evidence = new List<string> { "问题陈述", "上下文信息" },
                Alternatives = new List<string>(),
                IsConclusive = false
            };

            return step;
        }

        private async Task<ReasoningStep> PerformReasoningStepAsync(
            int stepNumber, 
            string problem, 
            List<string> hypotheses, 
            List<string> evidence, 
            string previousConclusion)
        {
            await Task.Delay(10); // 模拟思考时间

            var stepType = DetermineStepType(stepNumber, hypotheses.Count);
            var reasoning = BuildReasoningText(hypotheses, evidence, previousConclusion);
            var newHypotheses = RefineHypotheses(hypotheses, evidence);
            var newEvidence = GatherEvidence(hypotheses, evidence);
            var alternatives = GenerateAlternatives(hypotheses);
            var conclusion = DrawConclusion(newHypotheses, newEvidence, stepNumber);
            var confidence = CalculateStepConfidence(newHypotheses, newEvidence, stepNumber);
            var isConclusive = IsConclusive(newHypotheses, newEvidence, stepNumber);

            return new ReasoningStep
            {
                StepNumber = stepNumber,
                Type = stepType,
                Description = GetStepDescription(stepType, stepNumber),
                Input = string.Join("; ", hypotheses),
                Reasoning = reasoning,
                Conclusion = conclusion,
                Confidence = confidence,
                GeneratedHypotheses = newHypotheses,
                Evidence = newEvidence,
                Alternatives = alternatives,
                IsConclusive = isConclusive
            };
        }

        private async Task<ReasoningStep> SynthesizeResultAsync(
            string problem, 
            List<ReasoningStep> steps, 
            List<string> evidence, 
            List<string> alternatives)
        {
            await Task.Delay(10); // 模拟思考时间

            var synthesis = $"基于{steps.Count}个推理步骤的综合分析：\n";
            synthesis += $"- 收集到{evidence.Count}项证据\n";
            synthesis += $"- 考虑了{alternatives.Count}个替代方案\n";
            synthesis += $"- 最高置信度步骤：{steps.Max(s => s.Confidence):F2}\n";
            
            var finalConclusion = steps.LastOrDefault()?.Conclusion ?? "无法得出明确结论";
            var bestHypothesis = steps
                .SelectMany(s => s.GeneratedHypotheses)
                .GroupBy(h => h)
                .OrderByDescending(g => g.Count())
                .FirstOrDefault()?.Key ?? "无";

            return new ReasoningStep
            {
                StepNumber = steps.Count + 1,
                Type = ReasoningStepType.Synthesis,
                Description = "最终综合和结论",
                Input = "所有推理步骤的结果",
                Reasoning = synthesis,
                Conclusion = $"最终结论：{finalConclusion}。最佳假设：{bestHypothesis}",
                Confidence = CalculateOverallConfidence(steps),
                GeneratedHypotheses = new List<string> { bestHypothesis },
                Evidence = evidence.Take(5).ToList(), // 取前5个最重要的证据
                Alternatives = alternatives.Take(3).ToList(), // 取前3个替代方案
                IsConclusive = true
            };
        }

        private List<string> GenerateInitialHypotheses(string problem, string context, List<string> goals)
        {
            var hypotheses = new List<string>();
            
            // 基于问题类型生成假设
            if (problem.Contains("如何") || problem.Contains("怎样"))
            {
                hypotheses.Add("这是一个方法论问题，需要步骤化解决方案");
                hypotheses.Add("可能存在多种解决路径");
            }
            
            if (problem.Contains("为什么") || problem.Contains("原因"))
            {
                hypotheses.Add("这是一个因果关系问题");
                hypotheses.Add("可能涉及多个影响因素");
            }
            
            if (problem.Contains("选择") || problem.Contains("决策"))
            {
                hypotheses.Add("这是一个决策问题，需要权衡利弊");
                hypotheses.Add("需要建立评估标准");
            }
            
            // 基于目标生成假设
            foreach (var goal in goals.Take(2))
            {
                hypotheses.Add($"解决方案应该满足目标：{goal}");
            }
            
            return hypotheses.Take(5).ToList();
        }

        private ReasoningStepType DetermineStepType(int stepNumber, int hypothesesCount)
        {
            if (stepNumber <= 2) return ReasoningStepType.Analysis;
            if (hypothesesCount > 3) return ReasoningStepType.Hypothesis;
            if (stepNumber % 2 == 0) return ReasoningStepType.Evaluation;
            return ReasoningStepType.Deduction;
        }

        private string GetStepDescription(ReasoningStepType type, int stepNumber)
        {
            return type switch
            {
                ReasoningStepType.Analysis => $"第{stepNumber}步：深入分析",
                ReasoningStepType.Hypothesis => $"第{stepNumber}步：假设生成",
                ReasoningStepType.Deduction => $"第{stepNumber}步：逻辑推导",
                ReasoningStepType.Evaluation => $"第{stepNumber}步：评估验证",
                ReasoningStepType.Synthesis => $"第{stepNumber}步：综合总结",
                _ => $"第{stepNumber}步：推理过程"
            };
        }

        private string BuildReasoningText(List<string> hypotheses, List<string> evidence, string previousConclusion)
        {
            var reasoning = "当前推理过程：\n";
            reasoning += $"基于前一步结论：{previousConclusion}\n";
            reasoning += $"考虑假设：{string.Join(", ", hypotheses.Take(3))}\n";
            reasoning += $"参考证据：{string.Join(", ", evidence.Take(3))}\n";
            return reasoning;
        }

        private List<string> RefineHypotheses(List<string> hypotheses, List<string> evidence)
        {
            var refined = new List<string>();
            
            // 保留最相关的假设
            foreach (var hypothesis in hypotheses.Take(3))
            {
                refined.Add($"精化假设：{hypothesis}");
            }
            
            // 基于证据生成新假设
            if (evidence.Count > 2)
            {
                refined.Add("基于累积证据的新假设");
            }
            
            return refined;
        }

        private List<string> GatherEvidence(List<string> hypotheses, List<string> existingEvidence)
        {
            var newEvidence = new List<string>();
            
            foreach (var hypothesis in hypotheses.Take(2))
            {
                newEvidence.Add($"支持{hypothesis}的证据");
            }
            
            return newEvidence;
        }

        private List<string> GenerateAlternatives(List<string> hypotheses)
        {
            var alternatives = new List<string>();
            
            foreach (var hypothesis in hypotheses.Take(2))
            {
                alternatives.Add($"替代方案：与{hypothesis}不同的方法");
            }
            
            return alternatives;
        }

        private string DrawConclusion(List<string> hypotheses, List<string> evidence, int stepNumber)
        {
            if (!hypotheses.Any()) return "无法得出结论";
            
            var mainHypothesis = hypotheses.First();
            var evidenceCount = evidence.Count;
            
            return $"基于{evidenceCount}项证据，{mainHypothesis}具有较高可信度";
        }

        private double CalculateStepConfidence(List<string> hypotheses, List<string> evidence, int stepNumber)
        {
            var baseConfidence = 0.5;
            var evidenceBonus = Math.Min(evidence.Count * 0.1, 0.3);
            var hypothesesPenalty = Math.Max((hypotheses.Count - 3) * 0.05, 0);
            var stepBonus = Math.Min(stepNumber * 0.02, 0.1);
            
            return Math.Min(baseConfidence + evidenceBonus - hypothesesPenalty + stepBonus, 0.95);
        }

        private bool IsConclusive(List<string> hypotheses, List<string> evidence, int stepNumber)
        {
            return hypotheses.Count <= 2 && evidence.Count >= 3 && stepNumber >= 3;
        }

        private double CalculateOverallConfidence(List<ReasoningStep> steps)
        {
            if (!steps.Any()) return 0.0;
            
            var avgConfidence = steps.Average(s => s.Confidence);
            var conclusiveSteps = steps.Count(s => s.IsConclusive);
            var conclusiveBonus = conclusiveSteps > 0 ? 0.1 : 0;
            
            return Math.Min(avgConfidence + conclusiveBonus, 0.95);
        }

        private double CalculateConfidence(List<ReasoningStep> steps, int evidenceCount, int alternativesCount)
        {
            var baseConfidence = CalculateOverallConfidence(steps);
            var evidenceBonus = Math.Min(evidenceCount * 0.02, 0.15);
            var alternativesBonus = Math.Min(alternativesCount * 0.01, 0.05);
            
            return Math.Min(baseConfidence + evidenceBonus + alternativesBonus, 0.98);
        }
    }

    /// <summary>
    /// 推理结果
    /// </summary>
    public class ReasoningResult
    {
        /// <summary>
        /// 问题描述
        /// </summary>
        public string Problem { get; set; } = string.Empty;
        
        /// <summary>
        /// 推理步骤列表
        /// </summary>
        public List<ReasoningStep> Steps { get; set; } = new();
        
        /// <summary>
        /// 最终结论
        /// </summary>
        public string FinalConclusion { get; set; } = string.Empty;
        
        /// <summary>
        /// 置信度
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// 证据列表
        /// </summary>
        public List<string> Evidence { get; set; } = new();
        
        /// <summary>
        /// 替代方案列表
        /// </summary>
        public List<string> Alternatives { get; set; } = new();
        
        /// <summary>
        /// 推理耗时
        /// </summary>
        public TimeSpan ReasoningTime { get; set; }
        
        /// <summary>
        /// 元数据
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    /// <summary>
    /// 推理步骤
    /// </summary>
    public class ReasoningStep
    {
        /// <summary>
        /// 步骤编号
        /// </summary>
        public int StepNumber { get; set; }
        
        /// <summary>
        /// 步骤类型
        /// </summary>
        public ReasoningStepType Type { get; set; }
        
        /// <summary>
        /// 步骤描述
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// 输入内容
        /// </summary>
        public string Input { get; set; } = string.Empty;
        
        /// <summary>
        /// 推理过程
        /// </summary>
        public string Reasoning { get; set; } = string.Empty;
        
        /// <summary>
        /// 结论
        /// </summary>
        public string Conclusion { get; set; } = string.Empty;
        
        /// <summary>
        /// 置信度
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// 生成的假设列表
        /// </summary>
        public List<string> GeneratedHypotheses { get; set; } = new();
        
        /// <summary>
        /// 证据列表
        /// </summary>
        public List<string> Evidence { get; set; } = new();
        
        /// <summary>
        /// 替代方案列表
        /// </summary>
        public List<string> Alternatives { get; set; } = new();
        
        /// <summary>
        /// 是否为结论性步骤
        /// </summary>
        public bool IsConclusive { get; set; }
    }

    /// <summary>
    /// 推理步骤类型
    /// </summary>
    public enum ReasoningStepType
    {
        /// <summary>
        /// 分析
        /// </summary>
        Analysis,
        
        /// <summary>
        /// 假设
        /// </summary>
        Hypothesis,
        
        /// <summary>
        /// 推导
        /// </summary>
        Deduction,
        
        /// <summary>
        /// 评估
        /// </summary>
        Evaluation,
        
        /// <summary>
        /// 综合
        /// </summary>
        Synthesis
    }
}